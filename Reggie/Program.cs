﻿using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Reggie
{
	// ReSharper disable once ClassNeverInstantiated.Global
	// ReSharper disable once ArrangeTypeModifiers
	partial class Program
	{
		// amount of characters to process at once
		private const int BlockSize = 5000;
		
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
				if (stream.CanSeek)
				{
					return stream.Position < stream.Length;
				}

				return stream.CanRead;
			}
			
			while (KeepReading(inStream))
			{
				var block = new byte[BlockSize];
				inStream.Read(block);

				var blockString    = Encoding.Default.GetString(block).Trim('\0');
				var replaced = Regex.Replace(blockString, parsedArgs.Expression, parsedArgs.ReplacePattern);
				outStream.Write(Encoding.Default.GetBytes(replaced));
			}
			inStream.Dispose();
			outStream.Dispose();
		}
	}
}