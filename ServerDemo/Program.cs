﻿using CSNet;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ServerDemo
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Server!");
			var server = new Server();
			var task = Task.Run(() => server.StartListen(1337, IPAddress.Loopback));
			Task.WaitAll(task);
		}
	}
}
