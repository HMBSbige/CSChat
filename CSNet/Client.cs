using CSChat.Common;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Common
{
	class Client
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
						Debug.WriteLine(@"Connection attempt " + attempts);

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
				Debug.WriteLine($@"Socket connected to {client.Socket.RemoteEndPoint}");

				// Start sending & receiving
				var sendThread = new Thread(() => Send(client));
				var receiveThread = new Thread(() => Receive(client));

				sendThread.Start();
				receiveThread.Start();

				// Listen for threads to be aborted (occurs when socket looses it's connection with the server)
				//Task.WaitAll(sendThread, receiveThread);
				while (sendThread.IsAlive && receiveThread.IsAlive)
				{

				}
			}
		}

		/// <summary>
		/// Sends a message to the server
		/// </summary>
		/// <param name="client"></param>
		private void Send(ConnectedObject client)
		{
			// Build message
			client.CreateOutgoingMessage($@"Message from {ClientName}");
			var data = client.OutgoingMessageToBytes();

			// Send it on a 1 second interval
			while (true)
			{
				Thread.Sleep(3000);
				try
				{
					client.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallback, client);
				}
				catch (SocketException)
				{
					Debug.WriteLine(@"Server Closed");
					client.Close();
					Thread.CurrentThread.Abort();
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
					Thread.CurrentThread.Abort();
				}
			}
		}

		/// <summary>
		/// Message sent handler
		/// </summary>
		/// <param name="ar"></param>
		private static void SendCallback(IAsyncResult ar)
		{
			Debug.WriteLine(@"Message Sent");
		}

		private static void Receive(ConnectedObject client)
		{
			var bytesRead = 0;

			while (true)
			{
				// Read message from the server
				try
				{
					bytesRead = client.Socket.Receive(client.Buffer, SocketFlags.None);
				}
				catch (SocketException)
				{
					Debug.WriteLine(@"Server Closed");
					client.Close();
					Thread.CurrentThread.Abort();
				}
				catch (Exception)
				{
					Thread.CurrentThread.Abort();
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
						Debug.WriteLine(@"Message Received");

						// Reset message
						client.ClearIncomingMessage();
					}
				}
			}
		}
	}
}