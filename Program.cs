using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using NDesk.Options;

namespace MoewBot
{
	internal class Program
	{
		static void Main(string[] args)
		{
			bool runProxy  = true;
			bool runServer = true;

			var p = new OptionSet()
			{
				{"no-proxy", v => runProxy = false},
				{"no-server", v => runServer = false}
			};
			var other = p.Parse(args);

			Console.Title = "MoewBot";

			Proxy  proxy  = new Proxy(65523);
			Server server = new Server(8085);

			if (runProxy)
			{
				Thread t = new Thread(ProxyStart);
				t.Start(proxy);
			}

			if (runServer)
			{
				var t = new Thread(ServerStart);
				t.Start(server);
			}

			AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
			{
				proxy.Stop();
				server.Stop();

				Logger.Close();
			};

			while (true)
			{
				var key = Console.ReadKey(true);

				switch (key.Key)
				{
					case ConsoleKey.Q:
						Environment.Exit(0);
						break;
					case ConsoleKey.R:
					{
						proxy?.CompileScripts();
						proxy?.LoadScripts();
					}
					break;
				}

				Thread.Sleep(50);
			}
		}

		private static void ServerStart(object o)
		{
			Server server = (Server) o;
			Thread.CurrentThread.Name = "Server";

			server.Start();
		}

		private static void ProxyStart(object o)
		{
			Proxy proxy = (Proxy) o;
			Thread.CurrentThread.Name = "Proxy";

			proxy.Start();
		}
	}
}