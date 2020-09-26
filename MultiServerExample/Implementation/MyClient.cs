using MultiServerExample.Base;
using System;

namespace MultiServerExample.Implementation
{
	public class MyClient : AsyncClientBase
	{
		public override void OnAttachedToServer()
		{
			base.OnAttachedToServer();

			Console.WriteLine($"{Id}: {GetType().Name} attached. Waiting for data...");
		}

		public override void OnDataReceived(byte[] buffer)
		{
			base.OnDataReceived(buffer);

			Console.WriteLine($"{Id}: {GetType().Name} recieved {buffer.Length} bytes. Writing 5 bytes back.");
			WriteData(new byte[] { 1, 2, 3, 4, 5 });
		}

		public override void OnDetachedFromServer(Exception ex)
		{
			base.OnDetachedFromServer(ex);

			Console.WriteLine($"{Id}: {GetType().Name} detached.");
		}
	}
}
