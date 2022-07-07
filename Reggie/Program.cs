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

			var regexEngine = new Regex(parsedArgs.Expression, parsedArgs.EngineFlags);

			if (parsedArgs.BlockSize <= 0)
				await ReplaceFull(parsedArgs, regexEngine);
			else await ReplaceBlocks(parsedArgs, regexEngine);
		}

		private static async Task<string> ReadToEndAsync(this Stream str)
		{
			using var sr = new StreamReader(str);
			return await sr.ReadToEndAsync();
		}

		private static async Task ReplaceFull(Args parsedArgs, Regex regexEngine)
		{
			var raw = parsedArgs.InFilePath == "-"
						  ? await Console.OpenStandardInput().ReadToEndAsync()
						  : await File.ReadAllTextAsync(parsedArgs.InFilePath);

			var replaced = regexEngine.Replace(raw, parsedArgs.ReplacePattern);

			if (parsedArgs.OutFilePath == "-")
				await Console.OpenStandardOutput().WriteAsync(Encoding.Default.GetBytes(replaced));
			else
				await File.WriteAllTextAsync(parsedArgs.OutFilePath, replaced);
		}

		private static async Task ReplaceBlocks(Args parsedArgs, Regex regexEngine)
		{
			await using var ins = parsedArgs.InFilePath == "-"
									  ? Console.OpenStandardInput()
									  : File.OpenRead(parsedArgs.InFilePath);

			await using var outs = parsedArgs.OutFilePath == "-"
									   ? Console.OpenStandardOutput()
									   : File.OpenWrite(parsedArgs.OutFilePath);

			while (true)
			{
				var block      = new byte[parsedArgs.BlockSize];
				var readAmount = await ins.ReadAsync(block);

				if (readAmount == 0) break;

				var blockString = Encoding.Default.GetString(block).TrimEnd('\0');

				var replaced = regexEngine.Replace(blockString, parsedArgs.ReplacePattern);

				await outs.WriteAsync(Encoding.Default.GetBytes(replaced));
			}
		}
	}
}