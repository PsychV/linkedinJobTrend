using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace protoType
{
    internal class TOR
    {

        public static HttpClient CreateHttpClientWithTor()
        {
            readyTorBrowser();

            var proxy = new WebProxy
            {
                Address = new Uri("socks5://localhost:9150")
            };

            var handler = new HttpClientHandler
            {
                Proxy = proxy
            };

            var client = new HttpClient(handler);

            var TorConnectionIsOn = TorConnection().Wait(TimeSpan.FromMinutes(5));

            return TorConnectionIsOn ? client : throw new Exception("Cannot connect to TOR and create HttpClient");
        }

        private static void readyTorBrowser()
        {
            var torProcess = new Process();
            torProcess.StartInfo.FileName = "D:\\Tor Browser\\Browser\\firefox.exe";
            torProcess.Start();
        }

        private static async Task TorConnection()
        {
            string ipAddress = "localhost";
            int port = 9150;

            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(ipAddress, port);
                    Console.WriteLine("Port {0} is open on {1}.", port, ipAddress);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Port {0} is closed on {1}.", port, ipAddress);
            }

        }
    }
}
