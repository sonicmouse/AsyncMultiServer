using MultiServerExample.Base;
using MultiServerExample.Implementation;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MultiServerExample
{
	class Program
	{
		static void Main(string[] args)
		{
			var servers = new IAsyncServerBase[]
			{
				new MyServer(50000),
				new MyServer(50001),
				new MyOtherServer(50002)
			};

			foreach (var s in servers)
			{
				s.StartListening();
			}

			RunTestUsingMyServer("1", 89, 50000);
			RunTestUsingMyServer("2", 127, 50001);
			RunTestUsingMyOtherServer("3", 88, 50002);

			Console.Write("Press any key to exit... ");
			Console.ReadKey(true);

			foreach (var s in servers)
			{
				s.WriteDataToAllClients(new byte[] { 1, 2, 3, 4, 5 });
				s.StopListening();
			}
		}

		private static void RunTestUsingMyServer(string name, int clientCount, int port)
		{
			Parallel.For(0, clientCount, x =>
			{
				using (var t = new TcpClient())
				{
					t.Connect(IPAddress.Loopback, port);
					t.GetStream().Write(new byte[] { 1, 2, 3, 4, 5 }, 0, 5);
					t.GetStream().Read(new byte[512], 0, 512);
					t.Close();
				}
				Console.WriteLine($"FINISHED PASS {name} #{x}");
			});
		}

		private static void RunTestUsingMyOtherServer(string name, int clientCount, int port)
		{
			Parallel.For(0, clientCount, x =>
			{
				using (var t = new TcpClient())
				{
					t.Connect(IPAddress.Loopback, port);
					t.GetStream().Read(new byte[512], 0, 512);
					t.GetStream().Write(new byte[] { 1, 2, 3, 4, 5, 6 }, 0, 6);
					t.Close();
				}
				Console.WriteLine($"FINISHED PASS {name} #{x}");
			});
		}
	}
}
