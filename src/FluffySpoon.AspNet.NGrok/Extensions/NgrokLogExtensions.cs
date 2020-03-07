using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FluffySpoon.AspNet.NGrok.Extensions
{
	public static class NGrokLogExtensions
	{
		public static Dictionary<string, string> ParseLogData(string input)
		{
			var result = new Dictionary<string, string>();
			var stream = new StringReader(input);
			int lastRead = 0;

			while (lastRead > -1)
			{
				// Read Key
				var keyBuilder = new StringBuilder();
				while (true)
				{
					lastRead = stream.Read();
					var c = (char)lastRead;
					if (c == '=')
					{
						break;
					}
					keyBuilder.Append(c);
				}

				// Read Value
				var valueBuilder = new StringBuilder();
				lastRead = stream.Read();
				var firstValChar = (char)lastRead;
				bool quoteWrapped = false;
				if (firstValChar == '"')
				{
					quoteWrapped = true;
					lastRead = stream.Read();
					valueBuilder.Append((char)lastRead);
				}
				else
				{
					valueBuilder.Append(firstValChar);
				}
				while (true)
				{
					lastRead = stream.Read();
					if (lastRead == -1)
					{
						break;
					}

					var c = (char)lastRead;
					if (quoteWrapped && c == '"')
					{
						lastRead = stream.Read();
						break;
					}
					if (!quoteWrapped && c == ' ')
					{
						break;
					}
					valueBuilder.Append(c);
				}

				result.Add(keyBuilder.ToString(), valueBuilder.ToString());
			}
			return result;
		}

		public static LogLevel ParseLogLevel(string logLevelRaw)
		{
			if (!string.IsNullOrWhiteSpace(logLevelRaw))
			{
				return LogLevel.Debug;
			}

			LogLevel logLevel;
			switch (logLevelRaw)
			{
				case "info":
					logLevel = LogLevel.Information;
					break;
				default:
					var parseResult = Enum.TryParse<LogLevel>(logLevelRaw, out logLevel);
					if (!parseResult)
					{
						logLevel = LogLevel.Debug;
					}
					break;
			}

			return logLevel;
		}

		public static string GetLogFormatString(Dictionary<string, string> logFormatData)
		{
			StringBuilder logFormatSB = new StringBuilder();
			foreach (var kvp in logFormatData)
			{
				logFormatSB.Append(kvp.Key);
				logFormatSB.Append(": {");
				logFormatSB.Append(kvp.Key);
				logFormatSB.Append("} | ");
			}
			var logFormatString = logFormatSB.ToString().TrimEnd(' ').TrimEnd('|').TrimEnd(' ');
			return logFormatString;
		}
	}
}