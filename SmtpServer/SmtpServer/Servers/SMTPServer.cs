using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SmtpServer.Server
{
    /// <summary>
    /// SMTP Server, tested with dotnets built in smtp wrappers.
    /// </summary>
    public class SMTPServer : ProtocolServer, IMailServer
    {
        public readonly char[] separators = new char[2] { '<', '>' };
        private const string mailFrom = "MAIL FROM:";
        private const string recipient = "RCPT TO:";
        private const string ehlo = "EHLO";
        private const string helo = "HELO";
        // RL this would be an sql db or something to that sort.
        private bool messageIncoming;


        public async Task Run()
        {
            await RunSmtp();
        }

        /// <summary>
        /// Handles SMTP protocol stubs: HELO, QUIT, RCPT, FROM and data.
        /// </summary>
        /// <returns></returns>
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
                        await Write("221 OK");
                        tcpClient.Close();
                        break;
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

        /// <summary>
        /// Parses stuff such as sender and recipient.
        /// </summary>
        private string ParseFromMessage(string strMessage, string msgType)
        {
            string actualValue = strMessage.Remove(0, (msgType + separators[0]).Length);
            int index = actualValue.IndexOf(separators[1]);
            actualValue = actualValue.Substring(0, index);
            return actualValue;
        }
    }
}
