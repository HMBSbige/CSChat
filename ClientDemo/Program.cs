using CSNet;
using System;
using System.Net;

namespace ClientDemo
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Client!");
			var client = new Client();
			client.Connect(new IPEndPoint(IPAddress.Loopback, 1337));
		}
	}
}
