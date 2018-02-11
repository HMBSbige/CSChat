using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace CSChat
{
    public class ConnectionManager
    {
        public IPAddress LocalIPAddress = IPAddress.Loopback;
        public int Port = 13337;
        public IPEndPoint EndPoint => new IPEndPoint(LocalIPAddress, Port);

        public Socket CreateListenerv4()
        {
            Socket socket = null;
            try
            {
                socket = CreateSocketv4();
                socket.Bind(EndPoint);
                socket.Listen(10);//参数：挂起的连接队列的最大长度
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Data.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return socket;
        }

        private Socket CreateSocketv4()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private Socket CreateSocketv6()
        {
            return new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}