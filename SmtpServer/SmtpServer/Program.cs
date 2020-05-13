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
        private static List<Email> receivedEmails;
        private static List<SMTPServer> servers;
        public static async Task Main(string[] args)
        {
            ServerHandler handler = new ServerHandler();

            await handler.RunServers();
        }

    }
}
