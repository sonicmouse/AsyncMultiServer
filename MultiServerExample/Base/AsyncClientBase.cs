using System;
using System.Net.Sockets;

namespace MultiServerExample.Base
{
	public interface IAsyncClientBase
	{
		event EventHandler<AsyncClientBaseErrorEventArgs> Error;
		void AttachClient(TcpClient client);
		void WriteData(byte[] data);
		void DetachClient(Exception ex);
		string Id { get; }
	}

	public abstract class AsyncClientBase : IAsyncClientBase
	{
		protected virtual int ReceiveBufferSize { get; } = 1024;
		private TcpClient Client { get; set; }
		private byte[] ReceiveBuffer { get; set; }
		public event EventHandler<AsyncClientBaseErrorEventArgs> Error;
		public string Id { get; }

		public AsyncClientBase()
		{
			Id = Guid.NewGuid().ToString();
		}

		public void AttachClient(TcpClient client)
		{
			if(ReceiveBuffer != null) { throw new InvalidOperationException(); }

			ReceiveBuffer = new byte[ReceiveBufferSize];
			Client = client;

			try
			{
				Client.GetStream().
					BeginRead(ReceiveBuffer, 0, ReceiveBufferSize, OnDataReceived, this);
				OnAttachedToServer();
			}
			catch (Exception ex)
			{
				Error?.Invoke(this,
					new AsyncClientBaseErrorEventArgs(ex, "BeginRead"));
			}
		}

		public void DetachClient(Exception ex)
		{
			try
			{
				Client.Close();
				OnDetachedFromServer(ex);
			}
			catch { /* intentionally swallow */ }
			
			Client = null;
			ReceiveBuffer = null;
		}

		public virtual void OnDataReceived(byte[] buffer) { }
		public virtual void OnAttachedToServer() { }
		public virtual void OnDetachedFromServer(Exception ex) { }

		public void WriteData(byte[] data)
		{
			try
			{
				Client.GetStream().BeginWrite(data, 0, data.Length, OnDataWrote, this);
			}
			catch(Exception ex)
			{
				Error?.Invoke(this, new AsyncClientBaseErrorEventArgs(ex, "BeginWrite"));
			}
		}

		private static void OnDataReceived(IAsyncResult iar)
		{
			var me = (AsyncClientBase)iar.AsyncState;

			if(me.Client == null) { return; }

			try
			{
				var bytesRead = me.Client.GetStream().EndRead(iar);
				var buf = new byte[bytesRead];
				Array.Copy(me.ReceiveBuffer, buf, bytesRead);

				me.OnDataReceived(buf);
			}
			catch (Exception ex)
			{
				me.Error?.Invoke(me, new AsyncClientBaseErrorEventArgs(ex, "EndRead"));
			}
		}

		private static void OnDataWrote(IAsyncResult iar)
		{
			var me = (AsyncClientBase)iar.AsyncState;
			try
			{
				me.Client.GetStream().EndWrite(iar);
			}
			catch(Exception ex)
			{
				me.Error?.Invoke(me,
					new AsyncClientBaseErrorEventArgs(ex, "EndWrite"));
			}
		}
	}
}
