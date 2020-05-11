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
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("localhost");
            MailMessage msg = new MailMessage("laa@blaa.com", "luu@blaa.com", "hoh", "paskaa");
            smtp.Send(msg);
            smtp.Dispose();
            //Pop3Client client = new Pop3Client("user ", "pw", "pop.gmail.com");
            
            Pop3Client client = new Pop3Client("yyy", "xxx", "localhost");
            Thread.Sleep(3000);
            client.Connect(false);
            string login = client.Login();
            if (!login.Contains("OK")) Console.WriteLine($"Ran into problems! Error info: {login}");
            string listResults = client.OpenInbox();
            if (!listResults.Contains("OK")) Console.WriteLine($"Ran into problems! Error info: {listResults}");
            string disconnect = client.Disconnect();
            if (!disconnect.Contains("OK")) Console.WriteLine($"Ran into problems quitting session! Error info: {disconnect}");
        }
    }
}
