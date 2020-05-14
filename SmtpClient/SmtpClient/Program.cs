using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;

namespace SmtpClient
{
    class Program
    {
        private static Pop3Client client;

        static void Main(string[] args)
        {
            //InitLocalTest();
            
            List<IEmailClient> emailClients = new List<IEmailClient>()
            {
                //new Pop3Client("yyy", "xxx", "localhost"),
                new IMapClient("xxxx", "yyyy", "imap.gmail.com"),
            };
            emailClients.ForEach(client =>
            {
                client.Connect();

                if (!client.Login())
                {
                    Console.WriteLine("ERROR!");
                    return;
                }
                string listResults = client.OpenInbox();
                listResults = client.OpenInbox(2);
                string disconnect = client.Disconnect();

            });
        }

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
