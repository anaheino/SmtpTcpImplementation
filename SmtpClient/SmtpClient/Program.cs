using System;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;

namespace SmtpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            while (true)
            {
            THIS IS HERE PURELY FOR TESTING SMTP SERVER EASILY
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("localhost");
                MailMessage msg = new MailMessage("laa@blaa.com", "luu@blaa.com", "hoh", "paskaa");
                smtp.Send(msg);//Handles all messages in the protocol
                Thread.Sleep(6000000);
                smtp.Dispose();//sends a Quit message    Console.WriteLine("Hello World!");

            }
            */
            Pop3Client client = new Pop3Client("xxx", "xxxx", "pop.gmail.com");
            client.Connect();
            string login = client.Login();
            if (!login.Contains("OK")) Console.WriteLine($"Ran into problems! Error info: {login}");
            string listResults = client.OpenInbox();
            if (!listResults.Contains("OK")) Console.WriteLine($"Ran into problems! Error info: {listResults}");
            string disconnect = client.Disconnect();
            if (!disconnect.Contains("OK")) Console.WriteLine($"Ran into problems quitting session! Error info: {disconnect}");

        }
    }
}
