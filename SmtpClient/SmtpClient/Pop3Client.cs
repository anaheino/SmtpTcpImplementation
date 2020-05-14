using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmtpClient
{
    internal class Pop3Client : MailClient, IEmailClient
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

        public async Task<string> OpenInbox(int index = 0)
        {
            string result = "";
            if (!tcpClient.Connected) return "ERROR! Not connected!";
            try
            {
                bool readMoreThanOneLine = false;
                if (index.Equals(0))
                {
                    await Write("LIST" + Environment.NewLine);
                    readMoreThanOneLine = true;
                }
                else
                {
                    await Write($"LIST {index}" + Environment.NewLine);
                } 
                result = await Read(readMoreThanOneLine);
            }
            catch (Exception e)
            {

            }
            return result;
        }

        public async Task Connect(bool ssl = true)
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

        public async Task<bool> Login()
        {
            bool loginSuccess = false;
            string resultString = "";
            if (!tcpClient.Connected) return false;
            while (!loginSuccess)
            {
                try
                {
                    resultString = await Read();
                    if (resultString.Length > 0)
                    {
                        if (resultString.Contains("PASS"))
                        {
                            await Write("PASS " + password + Environment.NewLine);
                        }
                        else if (resultString.Contains("Welcome"))
                        {
                            loginSuccess = true;
                        }
                        else if (resultString.Contains("+OK") && resultString.Contains("requests"))
                        {
                            await Write("USER " + user.Trim() + Environment.NewLine);
                        }
                    }
                }
                catch { }
            }
            
            return loginSuccess;
        }

        public async Task Write(string data)
        {
            var byteData = System.Text.Encoding.ASCII.GetBytes(data.ToCharArray());
            if (isSSL)
            {
                await sslStream.WriteAsync(byteData, 0, byteData.Length);
                await sslStream.FlushAsync();
            }
            else
            {
                await stream.WriteAsync(byteData, 0, byteData.Length);
                await stream.FlushAsync();
            }
        }

        public async Task<string> Disconnect()
        {
            await Write("QUIT" + Environment.NewLine);
            string result = await Read();
            tcpClient.Dispose();
            return result;
        }

        public async Task<string> Read(bool readMultipleLines = false)
        {
            string resultString = "";
            string line = "";
            if (readMultipleLines)
            {
                line = await streamReader.ReadLineAsync();
                resultString += line;
                while (!line.Equals(".") && !line.Contains("-ERR"))
                {
                    line = await streamReader.ReadLineAsync();
                    resultString += line;
                }
                return resultString;
            }
            resultString = await streamReader.ReadLineAsync();
            return resultString;
        }
    }
}