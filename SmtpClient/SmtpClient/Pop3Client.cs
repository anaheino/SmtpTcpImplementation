using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace SmtpClient
{
    internal class Pop3Client : IEmailClient
    {
        private string user;
        private string password;
        private string mailServer;
        private TcpClient tcpClient;
        private bool isSSL;
        private SslStream sslStream;
        private NetworkStream stream;
        private StreamReader streamReader;

        public Pop3Client(string user, string password, string mailServer)
        {
            this.user = user;
            this.password = password;
            this.mailServer = mailServer;
            tcpClient = new TcpClient();
        }

        public string OpenInbox(int index = 0)
        {
            string result = "";
            if (!tcpClient.Connected) return "ERROR! Not connected!";
            try
            {
                bool readMoreThanOneLine = false;
                if (index.Equals(0))
                {
                    Write("LIST" + Environment.NewLine);
                    readMoreThanOneLine = true;
                }
                else
                {
                    Write($"LIST {index}" + Environment.NewLine);
                } 
                result = Read(readMoreThanOneLine);
            }
            catch (Exception e)
            {

            }
            return result;
        }

        public void Connect(bool ssl = true)
        {
            isSSL = ssl;
            tcpClient.Connect(mailServer, 995);
            if (ssl)
            {
                sslStream = new SslStream(tcpClient.GetStream());
                sslStream.AuthenticateAsClient(mailServer);
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
            string resultString = "";
            if (!tcpClient.Connected) return false;
            while (!loginSuccess)
            {
                try
                {
                    resultString = Read();
                    if (resultString.Length > 0)
                    {
                        
                        if (resultString.Contains("PASS"))
                        {
                            Write("PASS " + password + Environment.NewLine);
                        }
                        else if (resultString.Contains("Welcome"))
                        {
                            loginSuccess = true;
                        }
                        else if (resultString.Contains("+OK") && resultString.Contains("requests"))
                        {
                            Write("USER " + user.Trim() + Environment.NewLine);
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

        public string Disconnect()
        {
            Write("QUIT" + Environment.NewLine);
            string result = Read();
            tcpClient.Dispose();
            return result;
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
    }
}