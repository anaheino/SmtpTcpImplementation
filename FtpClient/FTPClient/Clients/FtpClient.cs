using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FTPClient
{
    public class FtpClient
    {
        private string address;

        private TcpClient tcpClient { get; set; }
        private TcpClient dataClient { get; set; }
        private NetworkStream dataStream;
        private NetworkStream communicationStream;
        private StreamReader communicationStreamReader;
        private StreamReader dataReader;

        public FtpClient(string address, int port)
        {
            this.address = address;
            tcpClient = new TcpClient(address, port);
            communicationStream = tcpClient.GetStream();
            communicationStreamReader = new StreamReader(communicationStream);
        }

        public async Task Connect()
        {
            string msg = await Read();
            await Write("USER hullumies");
            msg = await Read();
            await Write("PASS hullunheina");
            msg = await Read(); 
        }

        internal async Task SetPassive()
        {
            await Write("EPSV");
            string msg = await Read();
            if (msg.Contains("Entering"))
            {
                CreateDataConnection(msg);
            }
        }

        private void CreateDataConnection(string msg)
        {
            string indexString = Regex.Match(msg.Substring(3), @"\d+").Value;
            int port = int.Parse(indexString);
            if (dataClient != null && dataClient.Connected) dataClient.Dispose();
            dataClient = new TcpClient(address, port);
            dataStream = dataClient.GetStream();
            dataReader = new StreamReader(dataStream);
        }

        internal async Task<string> ListContents(string filePath = "")
        {
            await Write($"LIST {filePath}");
            string msg = await Read();
            if (msg.ToUpper().Contains("Not found".ToUpper())) {
                return "Error, file not found";
            }
            string fileMsg = await Read(dataReader, true);

            msg = await Read();
            if (msg.ToLower().Contains("success")) return msg + ": " + fileMsg;
            else return "Error in process";
        }

        internal async Task<string> Retrieve(string filePath)
        {
            await SetPassive();
            await Write($"RETR {filePath}");
            string msg = await Read();
            if (msg.Contains("550") && msg.Contains("No such file") || msg.Contains("503") && msg.Contains("Bad"))
            {
                return $"ERROR: {msg}";
            }
            string dataMessage = await Read(dataReader);
            msg = await Read();
            // For this prototype situation, tested and retrieved only with .txt files.
            // didn't bother to save to byte array and file based on that, even though
            // I have done that before in work related issues. It's 2am on a thursday, gimme a break.
            if (msg.ToLower().Contains("success")) return msg + ", contents: " + dataMessage;
            return msg;
        }

        private async Task Write(string rawMessage)
        {
            byte[] msg = Encoding.ASCII.GetBytes(rawMessage + Environment.NewLine);
            await communicationStream.WriteAsync(msg, 0, msg.Length);
        }

        private async Task<string> Read(StreamReader streamReader = null, bool inQuotes = false)
        {
            if (streamReader == null)
            {
                streamReader = communicationStreamReader;
            }
            string msg = "";
            bool isCompleted = false;
            try
            {
                while (!isCompleted)
                {
                    string receivedMsg = await streamReader.ReadLineAsync();
                    if (receivedMsg.Length > 3)
                    {
                        string msgNumber = Regex.Match(receivedMsg.Substring(0, 3), @"\d+").Value;
                        if (!receivedMsg[3].Equals('-') && int.TryParse(msgNumber, out _))
                        {
                            isCompleted = true;
                        }
                        msg += receivedMsg;
                    }
                }
            }
            catch
            {
                // Data transfer over --> Server has forcibly disconnected the socket.
                // Ending in the try catch is pretty resource intense, so if this was an
                // actual RL application, this would be handled differently.
                return msg;
            }
            
            return msg;
        }

        internal async Task Disconnect()
        {
            await Write("QUIT");
            await Read();
            dataClient.Dispose();
            tcpClient.Dispose();
        }
    }
}
