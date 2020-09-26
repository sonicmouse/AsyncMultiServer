using MultiServerExample.Base;
using System;

namespace MultiServerExample.Implementation
{
	public class MyServer : AsyncServerBase<MyClient>
	{
		public MyServer(int port) : base(port)
		{
		}

		public override void OnClientConnected(MyClient client)
		{
			Console.WriteLine($"* MyClient connected with Id: {client.Id}");
			base.OnClientConnected(client);
		}

		public override void OnClientDisconnected(MyClient client, Exception ex)
		{
			Console.WriteLine($"***** MyClient disconnected with Id: {client.Id} ({ex.Message})");
			base.OnClientDisconnected(client, ex);
		}
	}
}
