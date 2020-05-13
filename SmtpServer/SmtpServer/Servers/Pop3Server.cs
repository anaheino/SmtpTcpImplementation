using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmtpServer.Server
{
    public class Pop3Server : ProtocolServer, IMailServer
    {
        private bool authenticated { get; set; }

        public async Task Run()
        {
            await RunPop3();
        }
        private async Task RunPop3()
        {
            string usernameMsg = "", passwordMsg = "", strMessage = String.Empty;
            await Write($"+OK Custom Pop ready for requests from {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString()}" + Environment.NewLine);

            while (true)
            {
                strMessage = await Read();
                if (strMessage.Length > 0)
                {
                    if (strMessage.Contains("USER"))
                    {
                        usernameMsg = strMessage;
                        await Write("+OK send PASS" + Environment.NewLine);
                    }
                    else if (strMessage.Contains("PASS"))
                    {
                        passwordMsg = strMessage;
                        if (userValidationOK(usernameMsg, passwordMsg))
                        {
                            await Write("+OK Welcome." + Environment.NewLine);
                            authenticated = true;
                        }
                    }
                    else if (strMessage.StartsWith("LIST"))
                    {
                        // this is cause we don't have an actual real inbox, so each message is marked as 100.
                        int mockSize = 100;
                        List<Email> emails = MailSingleton.emails;
                        if (strMessage.Any(c => char.IsDigit(c)))
                        {
                            string indexString = Regex.Match(strMessage, @"\d+").Value;
                            int index = int.Parse(indexString);
                            if (index > emails.Count - 1 || index.Equals(0))
                            {
                                await Write($"-ERR Index {index} out of range.");
                            }
                            else
                            {
                                await Write($"+OK {index} {mockSize}");
                            }
                        }
                        else
                        {
                            await Write($"+OK {emails.Count} {emails.Count * mockSize}");
                            await Write(".");

                        }
                    }
                    else if (strMessage.Contains("QUIT"))
                    {
                        await Write("+OK Farewell.");
                        break;
                    }
                }
            }
        }
        // mock implementation
        private bool userValidationOK(string user, string password)
        {
            return true;
        }

    }
}
