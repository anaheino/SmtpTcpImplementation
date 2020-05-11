using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
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
        // RL this would be an sql db or something to that sort.
        private TcpClient tcpClient;
        private NetworkStream stream;
        private bool messageIncoming;

        public bool isPop { get; internal set; }
        public SslStream SslStream { get; private set; }


        internal void Init(TcpClient client)
        {
            tcpClient = client;
            stream = tcpClient.GetStream();
        }


        public async Task Run()
        {
            if (isPop)
            {
                await RunPop3();
            }
            else
            {
                await RunSmtp();
            }

        }

        private async Task RunPop3()
        {
            string strMessage = String.Empty;
            await Write($"+OK Custom Pop ready for requests from {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()}" + Environment.NewLine);
            await AuthenticatePop3();
            while (true)
            {
                strMessage = await Read();
                if (strMessage.Length > 0)
                {
                    if (strMessage.StartsWith("LIST"))
                    {
                        // Handle list shit
                        if (strMessage.Any(c => char.IsDigit(c))) 
                        {

                        }
                        else
                        {
                            List<Email> emails = MailSingleton.emails;
                        }
                    }
                }
            }

        }

        private async Task AuthenticatePop3()
        {
            try
            {
                string user = await Read();
                await Write("+OK send PASS" + Environment.NewLine);
                string password = await Read();
                if (userValidationOK(user, password))
                {
                    await Write("+OK Welcome." + Environment.NewLine);
                }
                else
                {
                    // Stub, not triggered currently.
                    await Write("+ERROR." + Environment.NewLine);
                }
            }
            catch
            {
            }
        }

        // mock implementation
        private bool userValidationOK(string user, string password)
        {
            return true;
        }

        private async Task RunSmtp()
        {
            string strMessage = String.Empty;
            Email email = new Email();

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
                        email.Quit = strMessage;
                        tcpClient.Close();
                        break;//exit while
                    }
                    else if (strMessage.StartsWith(ehlo) || strMessage.StartsWith(helo))
                    {
                        email.Helo = strMessage;
                        await Write("250 OK");
                    }

                    else if (strMessage.StartsWith(recipient))
                    {
                        email.RecipientRaw = strMessage;
                        email.Recipient = ParseFromMessage(strMessage, recipient);
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
                        MailSingleton.emails.Add(email);
                    }
                }
            }
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
            ASCIIEncoding encoder = new ASCIIEncoding();
            bytesRead = await stream.ReadAsync(messageBytes, 0, 8192);
            string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
            return strMessage;
        }
    }
}
