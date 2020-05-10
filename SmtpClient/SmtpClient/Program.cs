using System;
using System.Net.Mail;
using System.Threading;

namespace SmtpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("localhost");
                MailMessage msg = new MailMessage("laa@blaa.com", "luu@blaa.com", "hoh", "paskaa");
                smtp.Send(msg);//Handles all messages in the protocol
                Thread.Sleep(6000000);
                smtp.Dispose();//sends a Quit message    Console.WriteLine("Hello World!");

            }
        }
    }
}
