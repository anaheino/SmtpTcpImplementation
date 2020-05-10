using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Server
{
    public class SMTPServer
    {
        public readonly char[] separators = new char[2] { '<', '>' };
        private const string mailFrom = "MAIL FROM:";
        private const string recipient = "RCPT TO:";
        private const string ehlo = "EHLO";
        private const string helo = "HELO";
        private const string plainTextSeparator = "quoted-printable";

        private TcpClient tcpClient;
        private Email email;
        private bool messageIncoming;

        internal void Init(TcpClient client)
        {
            tcpClient = client;
        }


        public async Task Run()
        {
            string strMessage = String.Empty;
            email = new Email();
            await Write("220 localhost -- Stub Email server");

            while (true)
            {
                try
                {
                    strMessage = await Read();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e.StackTrace}");
                    break;
                }

                if (strMessage.Length > 0)
                {
                    if (strMessage.StartsWith("QUIT"))
                    {
                        Console.WriteLine(strMessage);
                        email.Quit = strMessage;
                        tcpClient.Close();
                        break;//exit while
                    }
                    //message has successfully been received
                    else if (strMessage.StartsWith(ehlo) || strMessage.StartsWith(helo))
                    {
                        email.Helo = strMessage;
                        Console.WriteLine(strMessage);
                        await Write("250 OK");
                    }

                    else if (strMessage.StartsWith(recipient))
                    {
                        email.RecipientRaw = strMessage;
                        email.Recipient = ParseFromMessage(strMessage, recipient);
                        Console.WriteLine(strMessage);
                        await Write("250 OK");
                    }

                    else if (strMessage.StartsWith(mailFrom))
                    {
                        email.SenderRaw = strMessage;
                        email.Sender = ParseFromMessage(strMessage, mailFrom);
                        await Write("250 OK");
                    }

                    else if (strMessage.StartsWith("DATA"))
                    {
                        await Write("354 Start mail input; end with");
                        email.Data = await Read();
                        messageIncoming = true;
                        await Write("250 OK");
                    }
                    else if (messageIncoming)
                    {
                        email.TextPlain = strMessage;
                        messageIncoming = false;
                    }
                }
            }
        }

        private string ParseText(string data)
        {
            int index = data.IndexOf(plainTextSeparator);
            string ret = data.Substring(index, data.Length - index);
            return ret;
        }

        private string ParseFromMessage(string strMessage, string msgType)
        {
            string sender = strMessage.Remove(0, (msgType + separators[0]).Length);
            int index = sender.IndexOf(separators[1]);
            sender = sender.Substring(0, index);
            return sender;
        }

        private async Task Write(string strMessage)
        {
            var networkStream = tcpClient.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(strMessage + "\r\n");
            await networkStream.WriteAsync(buffer, 0, buffer.Length);
            await networkStream.FlushAsync();
        }

        private async Task<string> Read()
        {
            byte[] messageBytes = new byte[8192];
            int bytesRead = 0;
            NetworkStream clientStream = tcpClient.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            bytesRead = await clientStream.ReadAsync(messageBytes, 0, 8192);
            string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
            Console.WriteLine(strMessage);
            return strMessage;
        }
    }
}
