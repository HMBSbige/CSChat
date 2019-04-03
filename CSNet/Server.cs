using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CSNet
{
	public class Server
	{
		// Client Collection
		private readonly List<ConnectedObject> _clients;

		// Thread Signal
		private readonly ManualResetEvent _connected;

		// Server socket
		private static Socket _server;

		public Server()
		{
			_clients = new List<ConnectedObject>();
			_connected = new ManualResetEvent(false);
		}

		public void StartListen(int port, IPAddress ip)
		{
			try
			{
				var cm = new ConnectionManager
				{
					Port = port,
					LocalIPAddress = ip
				};

				Debug.WriteLine($@"[Server] [{ip}:{port}]:启动中...");
				_server = cm.CreateListener();
				Debug.WriteLine($@"[Server] [{ip}:{port}]:启动成功，等待连接...");

				while (true)
				{
					_connected.Reset();

					_server.BeginAccept(AcceptCallback, _server);

					_connected.WaitOne();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				StopListen();
			}
		}

		private void AcceptCallback(IAsyncResult ar)
		{
			Debug.WriteLine(@"[Server] 有客户端连接");

			_connected.Set();

			// Accept new client socket connection
			var socket = _server.EndAccept(ar);

			// Create a new client connection object and store the socket
			var client = new ConnectedObject
			{
				Socket = socket
			};

			// Store all clients
			_clients.Add(client);

			// Begin receiving messages from new connection
			try
			{
				client.Socket.BeginReceive(client.Buffer, 0, client.BufferSize, SocketFlags.None, ReceiveCallback, client);
			}
			catch (SocketException)
			{
				// Client was forcibly closed on the client side
				CloseClient(client);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			int bytesRead;

			// Check for null values
			if (!CheckState(ar, out var err, out var client))
			{
				Debug.WriteLine(err);
				return;
			}

			// Read message from the client socket
			try
			{
				bytesRead = client.Socket.EndReceive(ar);
			}
			catch (SocketException)
			{
				// Client was forcibly closed on the client side
				CloseClient(client);
				return;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
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
					client.PrintMessage();

					// Reset message
					client.ClearIncomingMessage();

					// Acknowledge message
					SendReply(client);
				}
			}

			// Listen for more incoming messages
			try
			{
				client.Socket.BeginReceive(client.Buffer, 0, client.BufferSize, SocketFlags.None, ReceiveCallback, client);
			}
			catch (SocketException)
			{
				// Client was forcibly closed on the client side
				CloseClient(client);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Sends a reply to client
		/// </summary>
		/// <param name="client"></param>
		private void SendReply(ConnectedObject client)
		{
			if (client == null)
			{
				Debug.WriteLine(@"[Server] 无法回应: client null");
				return;
			}

			Debug.Write(@"[Server] 发送回应: ");

			// Create reply
			client.CreateOutgoingMessage(@"消息收到");
			var byteReply = client.OutgoingMessageToBytes();

			// Listen for more incoming messages
			try
			{
				client.Socket.BeginSend(byteReply, 0, byteReply.Length, SocketFlags.None, SendReplyCallback, client);
			}
			catch (SocketException)
			{
				// Client was forcibly closed on the client side
				CloseClient(client);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Handler after a reply has been sent
		/// </summary>
		/// <param name="ar"></param>
		private static void SendReplyCallback(IAsyncResult ar)
		{
			Debug.WriteLine(@"[Server] 回应已发送");
		}

		/// <summary>
		/// Checks IAsyncResult for null value
		/// </summary>
		/// <param name="ar"></param>
		/// <param name="err"></param>
		/// <param name="client"></param>
		/// <returns></returns>
		private static bool CheckState(IAsyncResult ar, out string err, out ConnectedObject client)
		{
			// Initialise
			client = null;
			err = string.Empty;

			// Check ar
			if (ar == null)
			{
				err = @"Async result null";
				return false;
			}

			// Check client
			client = (ConnectedObject)ar.AsyncState;
			if (client == null)
			{
				err = @"Client null";
				return false;
			}

			return true;
		}

		/// <summary>
		/// 断开某个客户端
		/// </summary>
		/// <param name="client">客户端</param>
		private void CloseClient(ConnectedObject client)
		{
			Debug.WriteLine(@"[Server] 客户端断开连接");
			client.Close();
			if (_clients.Contains(client))
			{
				_clients.Remove(client);
			}
		}

		/// <summary>
		/// 停止监听，并断开所有客户端
		/// </summary>
		public void StopListen()
		{
			try
			{
				// Close all clients
				foreach (var connection in _clients)
				{
					//connection.Close();
					CloseClient(connection);
				}

				// Close server
				_server.Close();
			}
			catch
			{
				// ignored
			}
		}
	}
}
