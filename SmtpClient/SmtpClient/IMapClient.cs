using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace SmtpClient
{
    internal class IMapClient : IEmailClient
    {
        private string user;
        private string password;
        private string server;
        private TcpClient tcpClient;
        private bool isSSL;
        private SslStream sslStream;
        private NetworkStream stream;
        private StreamReader streamReader;
        public readonly int IMAP4_SSL = 993;
        public readonly int IMAP4 = 143;
        private int commandNumber = 0;

        public IMapClient(string user, string password, string server)
        {
            this.user = user;
            this.password = password;
            this.server = server;
            tcpClient = new TcpClient();
        }

        public void Connect(bool ssl = true)
        {
            isSSL = ssl;
            if (ssl) tcpClient.Connect(server, IMAP4_SSL);
            else tcpClient.Connect(server, IMAP4); 
            if (ssl)
            {
                sslStream = new SslStream(tcpClient.GetStream());
                sslStream.AuthenticateAsClient(server);
                streamReader = new StreamReader(sslStream);
            }
            else
            {
                stream = tcpClient.GetStream();
                streamReader = new StreamReader(stream);
            }
        }

        public bool Login()
        {
            bool loginSuccess = false;
            if (!tcpClient.Connected) return false;
            string msgNumber = GetCommandNumber();
            while (!loginSuccess)
            {
                try
                {
                    string resultString = Read();
                    if (resultString.Length > 0)
                    {
                        if (resultString.Contains("OK") && resultString.Contains("requests"))
                        {
                            Write(msgNumber + $" LOGIN " + user.Trim() + " " + password + Environment.NewLine);
                        }
                        else if (resultString.Contains($"{msgNumber} OK {user}"))
                        {
                            loginSuccess = true;
                        }
                    }
                }
                catch { }
            }
            return loginSuccess;
        }

        public void Write(string data)
        {
            var byteData = System.Text.Encoding.ASCII.GetBytes(data.ToCharArray());
            if (isSSL)
            {
                sslStream.Write(byteData, 0, byteData.Length);
                sslStream.Flush();
            }
            else
            {
                stream.Write(byteData, 0, byteData.Length);
                stream.Flush();
            }
        }

        public string Read(bool readMultipleLines = false)
        {
            string resultString = "";
            string line = null;
            if (readMultipleLines)
            {
                line = streamReader.ReadLine();
                resultString += line;
                while (!line.Equals(".") && !line.Contains("-ERR"))
                {
                    line = streamReader.ReadLine();
                    resultString += line;
                }
                return resultString;
            }
            resultString = streamReader.ReadLine();
            return resultString;
        }

        public string OpenInbox(int index = 0)
        {
            // IMAP doesn't support index 0, so if they parametrized it, change to 1.
            if (index.Equals(0)) index++;
            
            string msgNumber = GetCommandNumber();
            Write($"{msgNumber} LIST \"\" *" + Environment.NewLine);
            string contents = ReadCommandResults(msgNumber);
            msgNumber = GetCommandNumber();
            Write($"{msgNumber} Select Inbox" + Environment.NewLine);
            string response = ReadCommandResults(msgNumber);
            msgNumber = GetCommandNumber();
            Write($"{msgNumber} FETCH {index} FULL" + Environment.NewLine);
            return ReadCommandResults(msgNumber);
        }

        private string ReadCommandResults(string msgNumber)
        {
            bool commandEnded = false;
            string result = "";
            while (!commandEnded)
            {
                string line = Read();
                if (line.Length > 0)
                {
                    if (line.Contains(msgNumber) && line.Contains("Success"))
                    {
                        commandEnded = true;
                    }
                    else
                    {
                        result += line;
                    }
                }
            }
            return result;
        }

        public string Disconnect()
        {
            string msgNumber = GetCommandNumber();
            Write($"{msgNumber} LOGOUT" + Environment.NewLine);
            string logout = "";
            bool commandEnded = false;
            while (!commandEnded)
            {
                string line = Read();
                if (line.Length > 0)
                {
                    if (line.Contains(msgNumber) && line.Contains("Success"))
                    {
                        commandEnded = true;
                        logout = line;
                    }
                    else
                    {
                        logout += line;
                    }
                }
            }
            tcpClient.Dispose();
            return logout;
        }

        private string GetCommandNumber()
        {
            if (commandNumber >= 99 && commandNumber < 1000)
            {
                return "A" + (++commandNumber).ToString();
            }
            else if (commandNumber >= 1000)
            {
                commandNumber = 0;
                return "A00" + (++commandNumber).ToString();
            }
            else if (commandNumber >= 9)
            {
                return "A0" + (++commandNumber).ToString();
            }
            else
            {
                return "A00" + (++commandNumber).ToString();
            }
        }
    }
}