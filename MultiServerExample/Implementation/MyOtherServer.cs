using MultiServerExample.Base;
using System;

namespace MultiServerExample.Implementation
{
	public class MyOtherServer : AsyncServerBase<MyOtherClient>
	{
		public MyOtherServer(int port) : base(port)
		{
		}

		public override void OnClientConnected(MyOtherClient client)
		{
			Console.WriteLine($"* MyOtherClient connected with Id: {client.Id}");
			base.OnClientConnected(client);
		}


		public override void OnClientDisconnected(MyOtherClient client, Exception ex)
		{
			Console.WriteLine($"***** MyOtherClient disconnected with Id: {client.Id} ({ex.Message})");
			base.OnClientDisconnected(client, ex);
		}
	}
}
