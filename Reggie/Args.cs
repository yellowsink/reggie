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
			var positionalArgs = args.Where(a => a[0] != '-').ToArray();

			for (var i = 0; i < optionArgs.Length; i++)
			{
				var arg = optionArgs[i];

				if (arg.StartsWith("-f="))
				{
					// regex engine flags!
					var flags = arg[3..];
					foreach (var flag in flags)
					{
						RegexOptions? resolved = flag switch
						{
							'i' => RegexOptions.IgnoreCase,
							'm' => RegexOptions.Multiline,
							's' => RegexOptions.Singleline,
							'n' => RegexOptions.ExplicitCapture,
							'x' => RegexOptions.IgnorePatternWhitespace,
							'e' => RegexOptions.ECMAScript,
							'j' => RegexOptions.Compiled,
							_   => null
						};

						if (resolved == null)
						{
							Console.WriteLine($"Invalid regex engine flag {flag}");
							Environment.Exit(1);
						}

						parsed.EngineFlags |= resolved.Value;
					}

					continue;
				}

				if (arg.StartsWith("-b=") || arg.StartsWith("--blocksize="))
				{
					var numStr = arg.Split("=")[1];
					if (int.TryParse(numStr, out var num))
						parsed.BlockSize = num;
					else
					{
						Console.WriteLine("Invalid block size");
						Environment.Exit(1);
					}
				}
			}

			if (positionalArgs.Length != 4)
			{
				Console.WriteLine($"Expected 4 positional args but got {positionalArgs.Length}.");
				Environment.Exit(1);
			}

			parsed.InFilePath     = args[0];
			parsed.Expression     = args[1];
			parsed.ReplacePattern = args[2];
			parsed.OutFilePath    = args[3];

			return parsed;
		}

		private static void WriteHelp() => Console.WriteLine(@"
reggie <OPTIONS> <input file> <regex expression> <regex replace pattern> <output file>

Use a - for the filename to use stdin and stdout.

OPTIONS:
  -b=n --blocksize=n - use blocks of n bytes instead of the whole file
                       useful for huge files but some matches may be missed
                       requires the in and out streams to be different as both will be opened simultaneously

  -f=imsnxej         - regex engine flags
	more info can be found at https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options
	i - case insensitive          - should be obvious enough no?
	m - multiline mode            - ^ and $ match each line instead of the whole input string
	s - singleline mode           - . matches newlines as well (it usually doesn't)
	n - explicit capture          - do not capture unnamed groups - only capture named groups of form ""(?<mynamedgroup>expression)""
	x - ignore pattern whitespace - exclude unescaped whitespace from the pattern, any text in the pattern after a # is ignored
	e - ECMAscript behavior       - the regex engine behaves in an ECMAscript-compliant way
	j - JIT                       - performs Just-in-Time compilation on your expression, to speed up execution.
	                                adds significant startup lag, so only use if you have huge quantities of data to process.");
	}

	internal class Args
	{
		public int          BlockSize;
		public RegexOptions EngineFlags;
		public string       Expression     = string.Empty;
		public string       InFilePath     = string.Empty;
		public string       OutFilePath    = string.Empty;
		public string       ReplacePattern = string.Empty;
	}
}