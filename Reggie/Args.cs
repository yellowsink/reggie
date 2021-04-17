using System;
using System.IO;
using System.Linq;

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
-i --stdin  uses stdin instead of a file
-o --stdout uses stdout instead of a file");
		}
	}

	internal class Args
	{
		public bool   UseStdIn       = false;
		public bool   UseStdOut      = false;
		public string InFilePath     = string.Empty;
		public string OutFilePath    = string.Empty;
		public string Expression     = string.Empty;
		public string ReplacePattern = string.Empty;
	}
}