using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MoewBot
{
	public static class ConsoleEx
	{
		private const string EXCEPTION_INPUT_NULL = "Input is null";
		private const string EXCEPTION_INPUT_EMPTY = "Input is an empty string";

		public static readonly char[] CustomChars = "abcdefr".ToCharArray();

		private static readonly object ConsoleWriterLock = new object();

		private static void Write(string format, TextWriter writer, params object[] args)
		{
			lock (ConsoleWriterLock)
			{
				var re    = new Regex(@"(?<![&@])([@&][a-fA-FrR0-9])");
				var parts = re.Split(string.Format(format, args));

				foreach (var part in parts)
				{
					if (part == "")
						continue;

					if (re.IsMatch(part))
					{
						var type  = part.Substring(0, 1);
						var arg   = part.ToLower().Substring(1, 1);
						var color = ConsoleColor.Gray;

						if (int.TryParse(arg, out var num))
						{
							color = (ConsoleColor) num;
						}
						if (CustomChars.Contains(arg[0]))
						{
							if (arg == "r")
							{
								Console.ResetColor();
								continue;
							}

							color = ConsoleColor.Green + (arg[0] - 'a');
						}

						switch (type)
						{
							case "@":
								Console.BackgroundColor = color;
								break;
							case "&":
								Console.ForegroundColor = color;
								break;
						}
					}
					else
					{
						writer.Write(part);
					}
				}
			}
		}

		public static void Write(string format, params object[] args)
		{
			Write(format, Console.Out, args);
		}

		public static void WriteLine(string format, params object[] args)
		{
			Write(format + "\n", Console.Out, args);
		}


		public static void WriteError(string format, params object[] args)
		{
			Write(format, Console.Error, args);
		}

		public static void WriteErrorLine(string format, params object[] args)
		{
			Write(format + "\n", Console.Error, args);
		}


		public static int ReadInt()
		{
			var re     = new Regex(@"(\d+)");
			var groups = re.Match(Console.ReadLine() ?? throw new InvalidOperationException(EXCEPTION_INPUT_NULL)).Groups;
			var res    = "";

			if (groups.Count > 0)
				res = groups[0].Value;

			return int.Parse(res);
		}

		public static float ReadFloat()
		{
			var re     = new Regex(@"(\d+[.,]\d+)");
			var groups = re.Match(Console.ReadLine() ?? throw new InvalidOperationException(EXCEPTION_INPUT_NULL)).Groups;
			var res    = "";

			if (groups.Count > 0)
				res = groups[0].Value.Replace(",", ".");

			return float.Parse(res);
		}

		public static double ReadDouble()
		{
			var re     = new Regex(@"(\d+[.,]\d+)");
			var groups = re.Match(Console.ReadLine() ?? throw new InvalidOperationException(EXCEPTION_INPUT_NULL)).Groups;
			var res    = "";

			if (groups.Count > 0)
				res = groups[0].Value.Replace(",", ".");

			return double.Parse(res);
		}

		public static char ReadChar()
		{
			var line = Console.ReadLine() ?? throw new InvalidOperationException(EXCEPTION_INPUT_NULL);

			if (line.Length == 0)
				throw new InvalidOperationException(EXCEPTION_INPUT_EMPTY);

			return line[0];
		}
	}
}