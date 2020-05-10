using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Server;
namespace SmtpServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var servers = new List<SMTPServer>();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 25);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                SMTPServer handler = new SMTPServer();
                servers.Add(handler);
                handler.Init(client);
                Thread thread = new Thread(new ThreadStart(async () => { await handler.Run(); }));
                thread.Start();
            }
        }
      
    }
}
