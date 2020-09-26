using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MultiServerExample.Base
{
	public interface IAsyncServerBase
	{
		void StartListening();
		bool IsListening { get; }
		void StopListening();
		void WriteDataToAllClients(byte[] data);
	}

	public abstract class AsyncServerBase<TClientBase> : IAsyncServerBase
		where TClientBase : IAsyncClientBase, new()
	{
		// implement a TcpListener to gain access to Active property
		private sealed class ActiveTcpListener : TcpListener
		{
			public ActiveTcpListener(IPAddress localaddr, int port)
				: base(localaddr, port) { }
			public bool IsActive => Active;
		}

		// our listener object
		private ActiveTcpListener Listener { get; }
		// our clients
		private ConcurrentDictionary<string, TClientBase> Clients { get; }

		// construct with a port
		public AsyncServerBase(int port)
		{
			Clients = new ConcurrentDictionary<string, TClientBase>();
			Listener = new ActiveTcpListener(IPAddress.Any, port);
		}

		// virtual methods for client action
		public virtual void OnClientConnected(TClientBase client) { }
		public virtual void OnClientDisconnected(TClientBase client, Exception ex) { }

		// start the server
		public void StartListening()
		{
			if(!IsListening)
			{
				Listener.Start();
				Listener.BeginAcceptTcpClient(OnAcceptedTcpClient, this);
			}
		}

		// check if the server is running
		public bool IsListening =>
			Listener.IsActive;

		// stop the server
		public void StopListening()
		{
			if (IsListening)
			{
				Listener.Stop();
				Parallel.ForEach(Clients, x => x.Value.DetachClient(null));
				Clients.Clear();
			}
		}

		// async callback for when a client wants to connect
		private static void OnAcceptedTcpClient(IAsyncResult res)
		{
			var me = (AsyncServerBase<TClientBase>)res.AsyncState;

			if (!me.IsListening) { return; }

			try
			{
				TcpClient client = null;
				try
				{
					client = me.Listener.EndAcceptTcpClient(res);
				}
				catch(Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Warning: unable to accept client:\n{ex}");
				}

				if(client != null)
				{
					// create a new client
					var t = new TClientBase();
					// set up error callbacks
					t.Error += me.OnClientBaseError;
					// notify client we have attached
					t.AttachClient(client);
					// track the client
					me.Clients[t.Id] = t;
					// notify we have a new connection
					me.OnClientConnected(t);
				}
			}
			finally
			{
				// if we are still listening, wait for another connection
				if(me.IsListening)
				{
					me.Listener.BeginAcceptSocket(OnAcceptedTcpClient, me);
				}
			}
		}

		// Event callback from a client that an error has occurred
		private void OnClientBaseError(object sender, AsyncClientBaseErrorEventArgs e)
		{
			var client = (TClientBase)sender;
			client.Error -= OnClientBaseError;

			OnClientDisconnected(client, e.Exception);

			client.DetachClient(e.Exception);
			Clients.TryRemove(client.Id, out _);
		}

		// utility method to write data to all clients connected
		public void WriteDataToAllClients(byte[] data)
		{
			Parallel.ForEach(Clients, x => x.Value.WriteData(data));
		}
	}
}
