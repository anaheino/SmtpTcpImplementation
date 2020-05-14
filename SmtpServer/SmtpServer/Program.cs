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
        public static void Main(string[] args)
        {
            ServerHandler handler = new ServerHandler();
            handler.RunServers();
        }
    }
}
