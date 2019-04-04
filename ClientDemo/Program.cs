using CSNet;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ClientDemo
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Client!");
			var client = new Client();

			Task.Run(() =>
			{
				client.Connect(new IPEndPoint(IPAddress.Loopback, 1337));
			});

			while (true)
			{
				if (client.IsConnected)
				{
					var str = Console.ReadLine();
					if (str != "!disconnect")
					{
						client.Send(str);
					}
					else
					{
						client.Dispose();
						break;
					}
				}
			}
		}
	}
}
