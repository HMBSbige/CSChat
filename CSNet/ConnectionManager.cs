using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace CSNet
{
	public class ConnectionManager
	{
		public IPAddress LocalIPAddress = IPAddress.Loopback;
		public int Port = 1337;
		public IPEndPoint EndPoint => new IPEndPoint(LocalIPAddress, Port);

		public Socket CreateListener()
		{
			Socket socket;
			try
			{
				socket = CreateSocket();
				socket.Bind(EndPoint);
				socket.Listen(10);//参数：挂起的连接队列的最大长度
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				throw;
			}
			return socket;
		}

		public static Socket CreateSocket()
		{
			return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}
	}
}