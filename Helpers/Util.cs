using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace MoewBot
{
	public static class Util
	{
		public static string ConvertNullOrEmptyTo(this string a, string b)
		{
			return string.IsNullOrEmpty(a) ? b : a;
		}

		public static string Sanitize(this string txt)
		{
			return txt.Replace("{", "{{").Replace("}", "}}");
		}

		private static readonly Random Random = new Random();

		public static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
			                            .Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		public static List<string> FindScripts(string directory)
		{
			List<string> temp = new List<string>();

			foreach (var file in Directory.GetFiles(directory))
			{
				if(file.EndsWith(".cs"))
				{
					temp.Add(file);
				}
			}

			return temp;
		}

		public static List<string> FindScriptsLINQ(string directory)
		{
			return Directory.GetFiles(directory).Where(file => file.EndsWith(".cs")).ToList();
		}

		public static List<string> FindDlls(string directory)
		{
			List<string> temp = new List<string>();

			foreach (var file in Directory.GetFiles(directory))
			{
				if(file.EndsWith(".dll"))
				{
					temp.Add(file);
				}
			}

			return temp;
		}

		public static string CalculateMd5(string filename)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(filename))
				{
					var hash = md5.ComputeHash(stream);
					return BitConverter.ToString(hash).Replace("-", "").ToLower();
				}
			}
		}
	}
}
