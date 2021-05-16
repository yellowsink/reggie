using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Reggie
{
	internal static partial class Program
	{
		private static void Main(string[] args)
		{
			var parsedArgs = ProcessArgs(args);

			var inStream = parsedArgs.UseStdIn
				? Console.OpenStandardInput()
				: File.OpenRead(parsedArgs.InFilePath);

			var outStream = parsedArgs.UseStdOut
				? Console.OpenStandardOutput()
				: File.OpenWrite(parsedArgs.OutFilePath);

			static bool KeepReading(Stream stream)
			{
				try
				{
					return stream.Position < stream.Length;
				}
				catch (NotSupportedException)
				{
					return stream.CanRead;
				}
			}
			

			var regexEngine = new Regex(parsedArgs.Expression, parsedArgs.EngineFlags);
			while (KeepReading(inStream))
			{
				string blockString;

				if (parsedArgs.BlockSize != 0)
				{
					var block = new byte[parsedArgs.BlockSize];
					inStream.Read(block);

					blockString = Encoding.Default.GetString(block).Trim('\0');

					if (blockString.Length == 0)
						break; // we only got nulls
				}
				else
					blockString = File.ReadAllText(parsedArgs.InFilePath);
				
				var replaced = regexEngine.Replace(blockString, parsedArgs.ReplacePattern);
				if (parsedArgs.BlockSize != 0)
					outStream.Write(Encoding.Default.GetBytes(replaced));
				else
					File.WriteAllText(parsedArgs.OutFilePath, replaced);
			}
			inStream.Dispose();
			outStream.Dispose();
		}
	}
}