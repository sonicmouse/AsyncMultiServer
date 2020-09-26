using MultiServerExample.Base;
using System;

namespace MultiServerExample.Implementation
{
	public class MyOtherClient : AsyncClientBase
	{
		public override void OnAttachedToServer()
		{
			base.OnAttachedToServer();

			Console.WriteLine($"{Id}: {GetType().Name} attached. Writing 4 bytes back.");
			WriteData(new byte[] { 1, 2, 3, 4 });
		}

		public override void OnDataReceived(byte[] buffer)
		{
			base.OnDataReceived(buffer);

			Console.WriteLine($"{Id}: {GetType().Name} recieved {buffer.Length} bytes.");
		}

		public override void OnDetachedFromServer(Exception ex)
		{
			base.OnDetachedFromServer(ex);

			Console.WriteLine($"{Id}: {GetType().Name} detached.");
		}
	}
}
