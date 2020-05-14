using SmtpServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmtpServer
{
    public class IMapServer : ProtocolServer, IMailServer
    {

        public async Task Run()
        {
            await RunIMap();
        }
        /// <summary>
        /// Handles all implemented IMAP command stubs: LOGIN, LIST, SELECT, LOGOUT and FETCH.
        /// </summary>
        /// <returns></returns>
        private async Task RunIMap()
        {
            string username = "", password = "", strMessage = String.Empty;
            await Write($"+OK Custom IMAP ready for requests from {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()}" + Environment.NewLine);
            string msgNumber = "";
            while (true)
            {
                strMessage = await Read();
                if (strMessage.Length > 0)
                {
                    msgNumber = strMessage.Substring(0, 4);
                    if (strMessage.Contains("LOGIN"))
                    {
                        string[] rawCreds = Regex.Split(strMessage, "LOGIN");
                        string[] creds = Regex.Split(rawCreds[1].Trim(), " ");
                        username = creds[0];
                        password = creds[1].Replace(Environment.NewLine, "");
                        await Write($"{msgNumber} OK {username} {password} authenticated (Success)" + Environment.NewLine);
                    }
                    else if (strMessage.Contains("LIST"))
                    {
                        // Stub implementation
                        await Write($"* LIST \"Inbox\" (\\HasChildren \\NoSelect)");
                        await Write($"{msgNumber} OK Success");
                    }
                    else if (strMessage.Contains("SELECT"))
                    {
                        await Write($"{msgNumber} OK [READ-WRITE] Inbox selected. (Success)");
                    }
                    else if (strMessage.Contains("FETCH") && strMessage.Contains("FULL"))
                    {
                        // stub, because an actual implementation would take a quite lot more effort.
                        Email email = MailSingleton.emails[0];
                        await Write($"1 FETCH Recipient: {email.Recipient}, Sender: {email.Sender}, subject:{email.TextPlain}, {email.Data}");
                        await Write($"{msgNumber} OK Success");
                    }
                    else if (strMessage.Contains("LOGOUT"))
                    {
                        await Write("* BYE LOGOUT Requested");
                        await Write($"{msgNumber} OK 73 good day (Success)");
                        break;
                    }
                }
            }
        }
    }
}