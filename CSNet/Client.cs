using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CSNet
{
	public class Client : IDisposable
	{
		public string ClientName = @"Client";

		public ConnectedObject client;
		public bool IsConnected { private set; get; }

		protected ConnectedObject CreateNewSocket()
		{
			return new ConnectedObject { Socket = ConnectionManager.CreateSocket() };
		}

		public void Connect(EndPoint ipe)
		{
			// 尝试重连
			while (!_disposed)
			{
				// 创建新的 socket
				client = CreateNewSocket();

				// 连接次数
				var attempts = 0;

				// 不停尝试连接，直到连接到服务器
				while (!client.Socket.Connected)
				{
					try
					{
						++attempts;
						Debug.WriteLine($@"[{ClientName}] 尝试第 {attempts} 次连接");

						// 尝试连接
						client.Socket.Connect(ipe);
					}
					catch (SocketException)
					{
						//ignored
					}
				}

				// 显示连接状态
				IsConnected = true;
				Debug.WriteLine($@"[{ClientName}] 已连接到 {client.Socket.RemoteEndPoint}");

				// 启动接收线程
				var receiveThread = new Task(Receive);

				receiveThread.Start();

				// 与服务器断开连接
				Task.WaitAll(receiveThread);
				IsConnected = false;
			}

			Debug.WriteLine($@"[{ClientName}] 已取消连接到 {client.Socket.RemoteEndPoint}");
		}

		public void Send(string str)
		{
			// Build message
			client.CreateOutgoingMessage($@"{str}");
			var data = client.OutgoingMessageToBytes();

			try
			{
				client.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, client);
			}
			catch (SocketException)
			{
				Debug.WriteLine($@"[{ClientName}] 与服务器断开连接 Send");
				client.Dispose();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

		}

		protected void SendCallback(IAsyncResult ar)
		{
			Debug.WriteLine($@"[{ClientName}] 消息已发送");
		}

		protected void Receive()
		{
			while (true)
			{
				// Read message from the server
				int bytesRead;
				try
				{
					bytesRead = client.Socket.Receive(client.Buffer, SocketFlags.None);
				}
				catch (SocketException)
				{
					Debug.WriteLine($@"[{ClientName}] 与服务器断开连接 Rec");
					client.Dispose();
					return;
				}
				catch (Exception)
				{
					return;
				}


				// Check message
				if (bytesRead > 0)
				{
					// Build message as it comes in
					client.BuildIncomingMessage(bytesRead);

					// Check if we received the full message
					if (client.MessageReceived())
					{
						// Print message to the console
						Debug.WriteLine($@"[{ClientName}] 消息已收到");

						// Reset message
						client.ClearIncomingMessage();
					}
				}
			}
		}

		private void DisConnect()
		{
			client.Dispose();
		}

		private bool _disposed;
		public void Dispose()
		{
			if (!_disposed)
			{
				DisConnect();
				_disposed = true;
			}
		}
	}
}