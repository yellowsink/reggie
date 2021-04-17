using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reggie
{
	// ReSharper disable once ClassNeverInstantiated.Global
	// ReSharper disable once ArrangeTypeModifiers
	partial class Program
	{
		private static Args ProcessArgs(string[] args)
		{
			if (args.Contains("-h") || args.Contains("--help"))
			{
				WriteHelp();
				Environment.Exit(0);
			}
			
			var parsed         = new Args();
			var nonOptionIndex = 0;
			foreach (var arg in args)
			{
				if (arg.StartsWith("-f="))
				{
					// regex engine flags!
					var options = RegexOptions.None;
					var          flags   = arg[Range.StartAt(3)];
					foreach (var flag in flags)
					{
						switch (flag)
						{
							case 'i':
								options |= RegexOptions.IgnoreCase;
								break;
							case 'm':
								options |= RegexOptions.Multiline;
								break;
							case 's':
								options |= RegexOptions.Singleline;
								break;
							case 'n':
								options |= RegexOptions.ExplicitCapture;
								break;
							case 'x':
								options |= RegexOptions.IgnorePatternWhitespace;
								break;
							case 'e':
								options |= RegexOptions.ECMAScript;
								break;
							default:
								Console.WriteLine($"Invalid regex engine flag {flag}");
								Environment.Exit(1);
								break;
						}
					}

					parsed.EngineFlags = options;
					
					continue;
				}
				
				switch (arg)
				{
					case "-i":
					case "--stdin":
						parsed.UseStdIn = true;
						nonOptionIndex++;
						continue;
					case "-o":
					case "--stdout":
						parsed.UseStdOut = true;
						continue;
					default:
						switch (nonOptionIndex)
						{
							// file
							case 0:
								parsed.InFilePath = arg;
								if (File.Exists(parsed.InFilePath)) break;
								Console.WriteLine("Input file does not exist");
								Environment.Exit(1);
								break;
							// regex expression
							case 1:
								parsed.Expression = arg;
								break;
							// replace pattern
							case 2:
								parsed.ReplacePattern = arg;
								break;
							// output file
							case 3:
								if (parsed.UseStdOut)
								{
									Console.WriteLine("Too many arguments supplied for options");
									Environment.Exit(1);
								}
								
								parsed.OutFilePath = arg;
								if (File.Exists(parsed.OutFilePath)) break;
								new FileInfo(parsed.OutFilePath).Create().Close();


								break;
						}

						nonOptionIndex++;
						continue;
				}
			}

			var neededParams = 4;
			if (parsed.UseStdIn) neededParams--;
			if (parsed.UseStdOut) neededParams--;
			if (args.Count(a => !a.StartsWith('-')) < neededParams)
			{
				Console.WriteLine("Not enough params");
				Environment.Exit(1);
			}

			return parsed;
		}

		private static void WriteHelp()
		{
			Console.WriteLine(@"
reggie <OPTIONS> <input file (omit if stdin)> <regex expression> <regex replace pattern> <output file (omit if stdout)> 
-i --stdin  - uses stdin instead of a file
-o --stdout - uses stdout instead of a file
-f=imsnxe   - regex engine flags
	more info can be found at https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options
	i - case insensitive          - should be obvious enough no?
	m - multiline mode            - ^ and $ match each line instead of the whole input string
	s - singleline mode           - . matches every character INCLUDING newlines - usually does not match newlines
	n - explicit capture          - do not capture unnamed groups - only capture numbered or named groups of form ""(?<name>expression)""
	x - ignore pattern whitespace - exclude unescaped whitespace from the pattern, any text in the pattern after a # is ignored
	e - ECMAscript behavior       - the regex engine behaves in an ECMAscript-compliant way");
		}
	}

	internal class Args
	{
		public bool         UseStdIn       = false;
		public bool         UseStdOut      = false;
		public string       InFilePath     = string.Empty;
		public string       OutFilePath    = string.Empty;
		public string       Expression     = string.Empty;
		public string       ReplacePattern = string.Empty;
		public RegexOptions EngineFlags    = RegexOptions.None;
	}
}