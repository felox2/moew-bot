using Fiddler;

namespace MoewBot
{
	public abstract class Script
	{
		public void Init()
		{
			FiddlerApplication.BeforeRequest += BeforeRequest;
			FiddlerApplication.BeforeResponse += BeforeResponse;
		}

		public abstract void BeforeRequest(Session oSession);

		public abstract void BeforeResponse(Session oSession);
	}
}
