using System;
using System.IO;
using System.Threading;

namespace MoewBot
{
	public enum LogLevel
	{
		Trace,
		Debug,
		Info,
		Warn,
		Error
	}

	public class Logger
	{
		public static string LogsDir = "Logs";
		public static LogLevel ConsoleLogLevel = LogLevel.Trace;
		public static string[,] Colors =
		{
			{ "&a", "&2" },
			{ "&d", "&5" },
			{ "&b", "&3" },
			{ "&e", "&6" },
			{ "&c", "&4" },
		};

		private static StreamWriter _writerAll;

		private static void _log(object data, LogLevel level)
		{
			Directory.CreateDirectory(LogsDir);

			if (_writerAll == null)
				_writerAll = new StreamWriter(Path.Combine(LogsDir, "all.txt")) {AutoFlush = true};

			var dateString = DateTime.Now;
			var threadName = Thread.CurrentThread.Name ?? "";

			_writerAll?.WriteLine($"[{dateString:hh:mm:ss.ffff}][{threadName.ConvertNullOrEmptyTo("Unknown")}][{level}]: {data}");

			if (level < ConsoleLogLevel) return;

			var primary = Colors[(int) level, 0];
			var secondary = Colors[(int) level, 1];

			var a = $"{secondary}[{primary}{dateString:hh:mm:ss}{secondary}]{(threadName == "" ? "" : $"[{primary}{threadName}{secondary}]")}[{primary}{level}{secondary}]: {primary}{data}&r";
			a = a.Replace("&s", secondary);
			a = a.Replace("&p", primary);

			ConsoleEx.WriteLine(a);
		}

		public static void Log(LogLevel level, object msg)
		{
			_log(msg, level);
		}

		public static void Info(object msg)
		{
			_log(msg, LogLevel.Info);
		}

		public static void Error(object msg)
		{
			_log(msg, LogLevel.Error);
		}

		public static void Debug(object msg)
		{
			_log(msg, LogLevel.Debug);
		}

		public static void Trace(object msg)
		{
			_log(msg, LogLevel.Trace);
		}

		public static void Warn(object msg)
		{
			_log(msg, LogLevel.Warn);
		}

		public static void Close()
		{
			_writerAll?.Close();
		}

		public static void Close(CtrlTypes ctrlType)
		{
			_writerAll?.WriteLine($"Exit event caught: {ctrlType}");
			_writerAll?.Close();
		}
	}
}
