using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace SmtpClient
{
    internal class Pop3Client
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

        internal string OpenInbox()
        {
            string result = "";
            if (!tcpClient.Connected) return "ERROR! Not connected!";
            try
            {
                Write("LIST" + Environment.NewLine);
                result = Read(true);
            }
            catch (Exception e)
            {

            }
            return result;
        }

        internal void Connect(bool ssl = true)
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

        internal string Login()
        {
            string resultString = "";
            try
            {
                if (!tcpClient.Connected) return "";
                resultString = Read();
                Write("USER " + user.Trim() + Environment.NewLine);
                resultString = Read();
                Write("PASS " + password + Environment.NewLine);
                resultString = Read();
            }
            catch (Exception e)
            {
                return $"Ran into problems while logging in! Latest received message: {resultString} StackTrace : {e.StackTrace}";
            }
            return resultString;
        }

        private void Write(string data)
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

        internal string Disconnect()
        {
            Write("QUIT" + Environment.NewLine);
            string result = Read();
            tcpClient.Dispose();
            return result;
        }

        private string Read(bool readToEnd = false)
        {
            string resultString = ""; 
            if (readToEnd)
            {
                string line = "";
                while (!line.Equals("."))
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