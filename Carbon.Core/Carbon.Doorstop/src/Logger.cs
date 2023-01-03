using System;
using System.IO;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Utility;

internal class Logger
{
	private static readonly string LogFile
		= Path.Combine(Context.Carbon, "logs", "Carbon.Doorstop.log");

	internal enum Severity
	{
		Error, Warning, Notice, None
	}

	static Logger()
	{
		if (!Directory.Exists(Path.GetDirectoryName(LogFile)))
			Directory.CreateDirectory(Path.GetDirectoryName(LogFile));
		if (File.Exists(LogFile)) File.Delete(LogFile);
	}

	internal static void Write(Severity severity, object message, Exception ex = null)
	{
		string formatted;
		string timestamp = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ";

		switch (severity)
		{
			case Severity.Error:
				if (ex != null)
					formatted = $"[e] {message} ({ex?.Message})\n{ex?.StackTrace}";
				else
					formatted = $"[e] {message}";
				break;

			case Severity.Warning:
				formatted = $"[w] {message}";
				break;

			case Severity.Notice:
				formatted = $"[i] {message}";
				break;

			case Severity.None:
				Console.WriteLine(message);
				return;

			default:
				throw new Exception($"Severity {severity} not implemented.");
		}

		Console.WriteLine(formatted);
		File.AppendAllText(LogFile, $"{timestamp}{formatted}" + Environment.NewLine);
	}

	internal static void None(object message)
		=> Write(Logger.Severity.None, message);

	internal static void Log(object message)
		=> Write(Logger.Severity.Notice, message);

	internal static void Warn(object message)
		=> Write(Logger.Severity.Warning, message);

	internal static void Error(object message, Exception ex = null)
		=> Write(Logger.Severity.Error, message, ex);
}
