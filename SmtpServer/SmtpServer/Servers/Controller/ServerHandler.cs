using SmtpServer.Server;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer
{
    internal class ServerHandler
    {
        private List<IMailServer> servers;

        public void RunServers()
        {
            // This is stupid temp solution for inbox. in RL, a db.
            MailSingleton.emails = new List<Email>();
            servers = new List<IMailServer>();
            List<Task> TaskList = new List<Task>();
            IPEndPoint pop3Endpoint = new IPEndPoint(IPAddress.Any, 995);

            var smtpTask = Task.Run(async () => {
                await RunSMTP();
            });
            var pop3Task = Task.Run(async () =>
            {
                await RunPop3();
            });
            
            TaskList.Add(smtpTask);
            TaskList.Add(pop3Task);
            Task.WaitAll(TaskList.ToArray());
        }

        private async Task RunSMTP()
        {
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

        public async Task RunPop3()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 995);
            TcpListener listener = new TcpListener(endPoint);
            listener.Start();

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Pop3Server handler = new Pop3Server();
                servers.Add(handler);
                handler.Init(client);
                Thread thread = new Thread(new ThreadStart(async () => { await handler.Run(); }));
                thread.Start();
            }
        }
    }
}