using System;
using System.Runtime.InteropServices;
using System.Text;
using WindowsInput;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

namespace MoewBot
{
	public class WsServer : WebSocketBehavior
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr SetActiveWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetCursorPos(int x, int y);


		private readonly InputSimulator _inputSimulator = new InputSimulator();

		protected override void OnError(ErrorEventArgs e)
		{
			base.OnError(e);

			Logger.Error(e.Message);
		}

		protected override void OnClose(CloseEventArgs e)
		{
			base.OnClose(e);

			Logger.Debug($"OnClose: {e.Code} - {e.Reason} (clean={e.WasClean})");
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			base.OnMessage(e);

			if (!e.IsText)
			{
				Logger.Warn($"Non-text message received: {e.Data}");
				return;
			}

			dynamic data = JsonConvert.DeserializeObject(e.Data);

			Logger.Debug($"OnMessage: {e.Data.Sanitize()}");

			switch (data.type.ToString())
			{
				case "keyboard":
				{
					Logger.Info($"Simulating keyboard input '{((string)data.text).Sanitize()}'");
					foreach (var c in data.text.ToString().ToCharArray())
					{
						_inputSimulator.Keyboard.TextEntry(c);
					}
				}
					break;

				case "mouse_move":
				{
					if (data.x == null && data.y == null)
					{
						Logger.Warn("Didn't receive enough arguments for mouse move");
						return;
					}

					var x = (int) data.x;
					var y = (int) data.y;

					Logger.Info($"Simulating mouse input {x}, {y}");

					SetCursorPos(x, y);
				}
					break;

				case "mouse_click":
				{
					var btn = data.button ?? "left";

					if(btn == "left")
						_inputSimulator.Mouse.LeftButtonClick();
					if(btn == "right")
						_inputSimulator.Mouse.RightButtonClick();

					Logger.Info($"Simulating mouse click {btn}");
				}
					break;

				case "window":
				{
					string windowClass = data.windowClass ?? "";
					string windowTitle = data.windowTitle ?? "";

					if (windowClass != "" || windowTitle != "")
					{
						var hwnd = FindWindow(windowClass, windowTitle);

						if (hwnd == IntPtr.Zero)
						{
							Logger.Info($"Window not found for class='{windowClass}' title='{windowTitle}'");
						}
						else
						{
							var sb = new StringBuilder(256);
							GetWindowText(hwnd, sb, 256);

							Logger.Info($"Window found: '{sb}' (0x{hwnd.ToString("X")})");

							Send(JsonConvert.SerializeObject(new { type = "window", window = hwnd.ToString("X") }));

							SetForegroundWindow(hwnd);
							SetActiveWindow(hwnd);
						}

						return;
					}

					Logger.Warn("Didn't receive enough arguments for window");
				}
					break;
			}
		}
	}
}