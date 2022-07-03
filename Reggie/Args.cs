using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reggie
{
	internal static partial class Program
	{
		private static Args ProcessArgs(string[] args)
		{
			if (args.Contains("-h") || args.Contains("--help") || args.Length == 0)
			{
				WriteHelp();
				Environment.Exit(0);
			}

			var parsed = new Args();

			var optionArgs     = args.Where(a => a[0] == '-').ToArray();
			var positionalArgs = args.Where(a => a[1] != '-').ToArray();

			for (var i = 0; i < optionArgs.Length; i++)
			{
				var arg = optionArgs[i];

				if (arg.StartsWith("-f="))
				{
					// regex engine flags!
					var flags = arg[Range.StartAt(3)];
					foreach (var flag in flags)
						switch (flag)
						{
							case 'i':
								parsed.EngineFlags |= RegexOptions.IgnoreCase;
								break;
							case 'm':
								parsed.EngineFlags |= RegexOptions.Multiline;
								break;
							case 's':
								parsed.EngineFlags |= RegexOptions.Singleline;
								break;
							case 'n':
								parsed.EngineFlags |= RegexOptions.ExplicitCapture;
								break;
							case 'x':
								parsed.EngineFlags |= RegexOptions.IgnorePatternWhitespace;
								break;
							case 'e':
								parsed.EngineFlags |= RegexOptions.ECMAScript;
								break;
							case 'j':
								parsed.EngineFlags |= RegexOptions.Compiled;
								break;
							default:
								Console.WriteLine($"Invalid regex engine flag {flag}");
								Environment.Exit(1);
								break;
						}

					continue;
				}

				switch (arg)
				{
					case "-i":
					case "--stdin":
						parsed.UseStdIn = true;
						continue;
					case "-o":
					case "--stdout":
						parsed.UseStdOut = true;
						continue;
					case "-b":
					case "--blocksize":
						if (!int.TryParse(optionArgs[i + 1], out var num))
						{
							Console.WriteLine("Invalid block size");
							Environment.Exit(1);
						}
						else { parsed.BlockSize = num; }

						continue;
					default:
						Console.WriteLine($"Did not understand flag \"{arg}\"");
						Environment.Exit(1);
						break;
				}
			}

			for (var i = 0; i < positionalArgs.Length; i++)
			{
				var arg = positionalArgs[i];

				switch (i)
				{
					case 0:
						if (!parsed.UseStdIn)
							parsed.InFilePath = arg;
						else
							parsed.Expression = arg;
						continue;
					case 1:
						if (!parsed.UseStdIn)
							parsed.Expression = arg;
						else
							parsed.ReplacePattern = arg;
						continue;
					case 2:
						if (!parsed.UseStdIn) { parsed.ReplacePattern    = arg; }
						else if (!parsed.UseStdOut) { parsed.OutFilePath = arg; }
						else
						{
							Console.WriteLine("Incorrect number of positional args for given options.");
							Environment.Exit(1);
						}

						continue;
					case 3:
						if (!parsed.UseStdIn && !parsed.UseStdOut) { parsed.OutFilePath = arg; }
						else
						{
							Console.WriteLine("Incorrect number of positional args for given options.");
							Environment.Exit(1);
						}

						continue;
					case 4:
						Console.WriteLine("Incorrect number of positional args.");
						Environment.Exit(1);
						continue;
				}
			}

			return parsed;
		}

		private static void WriteHelp() => Console.WriteLine(@"
reggie <OPTIONS> <input file (omit if stdin)> <regex expression> <regex replace pattern> <output file (omit if stdout)>

reggie -i -o <regex expression> <regex replace pattern>

-i   --stdin     - uses stdin instead of a file
-o   --stdout    - uses stdout instead of a file
-b n --blocksize - use blocks of n bytes instead of the whole file - useful for huge files but some matches may be missed

-f=imsnxej       - regex engine flags
	more info can be found at https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options
	i - case insensitive          - should be obvious enough no?
	m - multiline mode            - ^ and $ match each line instead of the whole input string
	s - singleline mode           - . matches every character INCLUDING newlines - usually does not match newlines
	n - explicit capture          - do not capture unnamed groups - only capture numbered or named groups of form ""(?<name>expression)""
	x - ignore pattern whitespace - exclude unescaped whitespace from the pattern, any text in the pattern after a # is ignored
	e - ECMAscript behavior       - the regex engine behaves in an ECMAscript-compliant way
	j - JIT                       - performs Just-in-Time compilation on your expression to machine code, to speed up execution.
	                                adds significant startup lag, so only use if you have multiple gigabytes of data to process.
	                                the replace string is unaffected as replace value can change dynamically with regex");
	}

	internal class Args
	{
		public int          BlockSize;
		public RegexOptions EngineFlags    = RegexOptions.None;
		public string       Expression     = string.Empty;
		public string       InFilePath     = string.Empty;
		public string       OutFilePath    = string.Empty;
		public string       ReplacePattern = string.Empty;
		public bool         UseStdIn;
		public bool         UseStdOut;
	}
}