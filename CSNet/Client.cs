using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CSNet
{
	public class Client
	{
		public string ClientName = @"Client";

		/// <summary>
		/// Attempts to connect to a server
		/// </summary>
		public void Connect(EndPoint ipe)
		{
			// Attempt to reconnect
			while (true)
			{
				ConnectedObject client = new ConnectedObject { Socket = ConnectionManager.CreateSocket() };
				// Create a new socket
				var attempts = 0;

				// Loop until we connect (server could be down)
				while (!client.Socket.Connected)
				{
					try
					{
						++attempts;
						Debug.WriteLine($@"[{ClientName}] 尝试第 {attempts} 次连接");

						// Attempt to connect
						client.Socket.Connect(ipe);
					}
					catch (SocketException)
					{
						//Console.Clear();
					}
				}

				// Display connected status
				//Console.Clear();
				Debug.WriteLine($@"[{ClientName}] 已连接到 {client.Socket.RemoteEndPoint}");

				// Start sending & receiving
				var sendThread = new Task(() => Send(client));
				var receiveThread = new Task(() => Receive(client));

				sendThread.Start();
				receiveThread.Start();

				// Listen for threads to be aborted (occurs when socket looses it's connection with the server)
				Task.WaitAll(sendThread, receiveThread);
			}
		}

		/// <summary>
		/// Sends a message to the server
		/// </summary>
		/// <param name="client"></param>
		private void Send(ConnectedObject client)
		{
			// Build message
			client.CreateOutgoingMessage($@"从 {ClientName} 发来的消息");
			var data = client.OutgoingMessageToBytes();

			// Send it on a 3 second interval
			while (true)
			{
				Thread.Sleep(3000);
				try
				{
					client.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, client);
				}
				catch (SocketException)
				{
					Debug.WriteLine($@"[{ClientName}] 与服务器断开连接 Send");
					client.Close();
					return;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					return;
				}
			}
		}

		/// <summary>
		/// Message sent handler
		/// </summary>
		/// <param name="ar"></param>
		private void SendCallback(IAsyncResult ar)
		{
			Debug.WriteLine($@"[{ClientName}] 消息已发送");
		}

		private void Receive(ConnectedObject client)
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
					client.Close();
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
	}
}