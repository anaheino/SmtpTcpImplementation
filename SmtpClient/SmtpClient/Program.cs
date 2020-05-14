using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpClient
{
    class Program
    {
        private static Pop3Client client;
        private static List<string> openInboxMsgs = new List<string>();
        private static List<string> disconnectMsgs = new List<string>();

        public static void Main(string[] args)
        {
            InitLocalTest();
            
            List<IEmailClient> emailClients = new List<IEmailClient>()
            {
                new Pop3Client("yyy", "xxx", "localhost"),
                new IMapClient("hahaaa", "laaaa", "localhost"),
            };

            List<Task> TaskList = new List<Task>()
            {
                GenerateClientTask(emailClients[0]),
                GenerateClientTask(emailClients[1]),
            };

            Task.WaitAll(TaskList.ToArray());

        }

        /// <summary>
        /// Starts the actual mail sending tasks.
        /// </summary>
        private static Task GenerateClientTask(IEmailClient emailClient)
        {
            return Task.Run(async () =>
            {
                var client = emailClient;
                await client.Connect(false);
                if (!await client.Login())
                {
                    Console.WriteLine("ERROR!");
                    return;
                }
                openInboxMsgs.Add(await client.OpenInbox());
                openInboxMsgs.Add(await client.OpenInbox(2));
                disconnectMsgs.Add(await client.Disconnect());
            });
        }

        /// <summary>
        /// Sends some messages to the SMTP server inbox, so we have something to query during this run.
        /// This is necessary cause currently mail is stored in a mock singleton, and not an actual db.
        /// </summary>
        private static void InitLocalTest()
        {
            for (int i = 0; i < 5; i++)
            {
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("localhost");
                MailMessage msg = new MailMessage("laa@blaa.com", "luu@blaa.com", "otsikko", $"onpakakkaa {i}");
                smtp.Send(msg);
                smtp.Dispose();
            }
            client = new Pop3Client("yyy", "xxx", "localhost");
        }
    }
}
