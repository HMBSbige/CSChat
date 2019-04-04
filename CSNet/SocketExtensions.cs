using System.Net.Sockets;

namespace CSNet
{
	static class SocketExtensions
	{
		public static bool IsConnected(this Socket socket, int microSeconds = 1000)
		{
			try
			{
				return !(socket.Poll(microSeconds, SelectMode.SelectRead) && socket.Available == 0);
			}
			catch (SocketException)
			{
				return false;
			}
		}
	}
}
