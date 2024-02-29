using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteInterface
{
    public class Client
    {
        public async Task ConnectAsync()
        {
            //using TcpClient client = new();
            //await client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 13);
            //await using NetworkStream stream = client.GetStream();
            
            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(IPAddress.Parse("127.0.0.1"), 8888);
            await using NetworkStream stream = new NetworkStream(socket);
            
            var buffer = new byte[1_024];
            int received = await stream.ReadAsync(buffer);

            var message = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine(message);
        }
    }
}