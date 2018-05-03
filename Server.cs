using WebSocketSharp.Server;

namespace MoewBot
{
	public class Server
	{
		public bool ShouldStop = false;

		private WebSocketServer _webSocketServer;
		private readonly short _port;
		private readonly string _path;

		public Server(short port, string path = "/moew")
		{
			_port = port;
			_path = path;
		}

		public void Start()
		{
			_webSocketServer = new WebSocketServer(_port);
			_webSocketServer.AddWebSocketService(_path, () => new WsServer());
			_webSocketServer.Start();

			Logger.Info($"WebSocket server listening on *:{_port}{_path}");
		}

		public void Stop()
		{
			_webSocketServer?.Stop();
		}
	}
}
