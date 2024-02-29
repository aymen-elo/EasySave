using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteInterface
{
    public class Server
    {
        public async Task StartAsync(int progress)
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888);
            using var socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ep);

            try
            {
                socket.Listen();
                using var acceptedSocket = await socket.AcceptAsync();
                using var stream = new NetworkStream(acceptedSocket, ownsSocket: true);

                var dateTimeBytes = Encoding.UTF8.GetBytes(progress.ToString());
                await acceptedSocket.SendAsync(dateTimeBytes, SocketFlags.None);

                Console.WriteLine($"Sent message: \"{progress}\"");
            }
            catch (SocketException)
            {
                socket.Dispose();
            }
        }
    }
}