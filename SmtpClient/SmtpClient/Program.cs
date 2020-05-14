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
            InitLocalTest();
            
            List<IEmailClient> emailClients = new List<IEmailClient>()
            {
                new Pop3Client("yyy", "xxx", "localhost"),
                new IMapClient("yyy", "xxx", "localhost"),
            };
            emailClients.ForEach(client =>
            {
                client.Connect(false);

                if (!client.Login())
                {
                    Console.WriteLine("ERROR!");
                    return;
                }
                string listResults = client.OpenInbox();
                if (!listResults.Contains("OK")) Console.WriteLine($"Ran into problems! Error info: {listResults}");

                listResults = client.OpenInbox(2);
                if (!listResults.Contains("OK")) Console.WriteLine($"Ran into problems! Error info: {listResults}");

                string disconnect = client.Disconnect();
                if (!disconnect.Contains("OK")) Console.WriteLine($"Ran into problems quitting session! Error info: {disconnect}");

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
