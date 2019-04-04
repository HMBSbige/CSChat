using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace CSNet
{
	public class ConnectedObject : IDisposable
	{
		#region Properties
		// UTF8 without BOM
		public readonly Encoding MessageEncoding = new UTF8Encoding(false);
		// Client socket
		public Socket Socket;
		// Size of receive buffer
		public int BufferSize = 1024;
		// Receive buffer
		public byte[] Buffer;
		// Received data string
		private StringBuilder IncomingMessage;
		// Message to be sent
		private StringBuilder OutgoingMessage;
		// Terminator for each message
		public string MessageTerminator = @"<END>";
		#endregion

		#region Constructors
		public ConnectedObject()
		{
			Buffer = new byte[BufferSize];
			IncomingMessage = new StringBuilder();
			OutgoingMessage = new StringBuilder();
		}
		#endregion

		#region Outgoing Message Methods
		/// <summary>
		/// Converts the outgoing message to bytes
		/// </summary>
		/// <returns></returns>
		public byte[] OutgoingMessageToBytes()
		{
			if (!OutgoingMessage.ToString().EndsWith(MessageTerminator))
			{
				OutgoingMessage.Append(MessageTerminator);
			}
			return MessageEncoding.GetBytes(OutgoingMessage.ToString());
		}


		/// <summary>
		/// Creates a new outgoing message
		/// </summary>
		/// <param name="msg"></param>
		public void CreateOutgoingMessage(string msg)
		{
			OutgoingMessage.Clear();
			OutgoingMessage.Append(msg);
			OutgoingMessage.Append(MessageTerminator);
		}

		#endregion

		#region Incoming Message Methods
		/// <summary>
		/// Converts the buffer to a string ans stores it
		/// </summary>
		public void BuildIncomingMessage(int bytesRead)
		{
			IncomingMessage.Append(MessageEncoding.GetString(Buffer, 0, bytesRead));
		}

		/// <summary>
		/// Determines if the message was fully received
		/// </summary>
		/// <returns></returns>
		public bool MessageReceived()
		{
			return IncomingMessage.ToString().EndsWith(MessageTerminator);
		}

		/// <summary>
		/// Clears the current incoming message so that we can start building for the next message
		/// </summary>
		public void ClearIncomingMessage()
		{
			IncomingMessage.Clear();
		}

		/// <summary>
		/// Gets the length of the incoming message
		/// </summary>
		/// <returns></returns>
		public int IncomingMessageLength()
		{
			return IncomingMessage.Length;
		}
		#endregion

		#region Connected Object Methods
		/// <summary>
		/// Closes the connection
		/// </summary>
		private void Close()
		{
			try
			{
				Socket.Shutdown(SocketShutdown.Both);
				Socket.Close();
			}
			catch (Exception)
			{
				Debug.WriteLine(@"连接已断开");
			}
		}

		public string GetRemoteEndPoint()
		{
			if (_disposed)
			{
				return null;
			}
			return Socket.RemoteEndPoint.ToString();
		}

		/// <summary>
		/// Print the details of the current incoming message
		/// </summary>
		public void PrintMessage()
		{
			var divider = new string('=', 60);
			Console.WriteLine();
			Console.WriteLine(divider);
			Console.WriteLine(@"收到消息");
			Console.WriteLine(divider);
			Console.WriteLine($@"从 {GetRemoteEndPoint()} 读取 {IncomingMessageLength()} 字节。");
			Console.WriteLine($@"消息: {IncomingMessage}");
		}
		#endregion

		#region Dispose

		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				Close();
				_disposed = true;
			}
		}
		#endregion

	}
}
