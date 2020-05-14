using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    public class FtpClient
    {
        private TcpClient tcpClient { get; set; }

        private NetworkStream communicationStream;

        private StreamReader communicationStreamReader;


        private StreamWriter communicationStreamWriter;

        public FtpClient(string address, int port, int dataport)
        {
            tcpClient = new TcpClient(address, port);
            communicationStream = tcpClient.GetStream();
            communicationStreamReader = new StreamReader(communicationStream);
            communicationStreamWriter = new StreamWriter(communicationStream);

        }

        public async Task Connect()
        {
            string msg = await Read();
            await Write("USER hullumies");
            msg = await Read();
            await Write("PASS xanadu92");
            msg = await Read(); 
        }

        private async Task Write(string rawMessage)
        {
            byte[] msg = Encoding.ASCII.GetBytes(rawMessage + Environment.NewLine);
            await communicationStream.WriteAsync(msg, 0, msg.Length);
        }

        private async Task<string> Read()
        {
            string msg = "";
            bool isCompleted = false;
            while (!isCompleted)
            {
                string receivedMsg = await communicationStreamReader.ReadLineAsync();
                if (!receivedMsg[3].Equals('-'))
                {
                    isCompleted = true;
                }
                msg += receivedMsg;
            }
            return msg;
        }

        internal async Task<string> Retrieve(string filePath)
        {
            await Write($"RETR {filePath}");
            string msg = await Read();
            if (msg.Contains("550") && msg.Contains("No such file"))
            {
                return msg;
            }
            return msg;
        }

        internal async Task Disconnect()
        {
            await Write("QUIT");
            await Read();
            tcpClient.Dispose();

        }
    }
}
