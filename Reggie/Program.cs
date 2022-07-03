using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reggie
{
	internal static partial class Program
	{
		private static async Task Main(string[] args)
		{
			var parsedArgs = ProcessArgs(args);

			await using var ins = parsedArgs.UseStdIn
									  ? Console.OpenStandardInput()
									  : File.OpenRead(parsedArgs.InFilePath);

			await using var outs = parsedArgs.UseStdOut
									   ? Console.OpenStandardOutput()
									   : File.OpenWrite(parsedArgs.OutFilePath);

			static bool KeepReading(Stream stream)
			{
				try { return stream.Position < stream.Length; }
				catch (NotSupportedException) { return stream.CanRead; }
			}


			var regexEngine = new Regex(parsedArgs.Expression, parsedArgs.EngineFlags);

			if (parsedArgs.BlockSize == 0)
			{
				using var       sr = new StreamReader(ins);
				await using var sw = new StreamWriter(outs);
				await sw.WriteAsync(regexEngine.Replace(await sr.ReadToEndAsync(), parsedArgs.ReplacePattern));

				return;
			}

			while (KeepReading(ins))
			{
				var block = new byte[parsedArgs.BlockSize];
				// ReSharper disable once MustUseReturnValue
				await ins.ReadAsync(block);

				var blockString = Encoding.Default.GetString(block).Trim('\0');

				if (blockString.Length == 0)
					break; // we only got nulls

				var replaced = regexEngine.Replace(blockString, parsedArgs.ReplacePattern);

				await outs.WriteAsync(Encoding.Default.GetBytes(replaced));
			}
		}
	}
}