using System;
using System.Runtime.InteropServices;
using System.Threading;
using NDesk.Options;

namespace MoewBot
{
	// An enumerated type for the control messages sent to the handler routine.
	public enum CtrlTypes
	{
		CTRL_C_EVENT = 0,
		CTRL_BREAK_EVENT,
		CTRL_CLOSE_EVENT,
		CTRL_LOGOFF_EVENT = 5,
		CTRL_SHUTDOWN_EVENT
	}

	internal class Program
	{
		// Declare the SetConsoleCtrlHandler function as external and receiving a delegate.
		[DllImport("Kernel32")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

		// A delegate type to be used as the handler routine for SetConsoleCtrlHandler.
		public delegate bool HandlerRoutine(CtrlTypes ctrlType);

		private static Proxy _proxy;
		private static Server _server;
		private static HandlerRoutine _handler;

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

			_handler = ConsoleCtrlCheck;
			SetConsoleCtrlHandler(_handler, true);

			Console.Title = "MoewBot";

			_proxy  = new Proxy(65523);
			_server = new Server(8085);

			if (runProxy)
			{
				var t = new Thread(ProxyStart);
				t.Start(_proxy);
			}

			if (runServer)
			{
				var t = new Thread(ServerStart);
				t.Start(_server);
			}

			while (true)
			{
				var key = Console.ReadKey(true);

				switch (key.Key)
				{
					case ConsoleKey.Q:
					{
						Exit(CtrlTypes.CTRL_CLOSE_EVENT);
						Environment.Exit(0);
					}
						break;
					case ConsoleKey.R:
					{
						_proxy?.CompileScripts();
						_proxy?.LoadScripts();
					}
						break;
				}

				Thread.Sleep(50);
			}
		}

		private static bool Exit(CtrlTypes ctrlType)
		{
			_proxy?.Stop();
			_server?.Stop();

			Logger.Close(ctrlType);

			return true;
		}

		private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
		{
			return Exit(ctrlType);
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