using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    internal class TftpClient
    {
        private string address;
        private int port;
        private UdpClient udpClient;

        public TftpClient(string address, int port)
        {
            this.address = address;
            this.port = port;
            udpClient = new UdpClient(address, port);
        }

        internal async Task Get()
        {
            await Write(1, "joku.txt");
            string msg = await Read();
        }

        private async Task Write(short type, string fileName)
        {
            byte[] opCode = BitConverter.GetBytes(type);
            byte[] filenameBytes = Encoding.ASCII.GetBytes(fileName);
            
            List<byte> actualMessage = opCode.ToList();
            actualMessage.AddRange(filenameBytes.ToList());
            actualMessage.Add(0);
            actualMessage.AddRange(Encoding.ASCII.GetBytes("netascii").ToList());
            actualMessage.Add(0);

            await udpClient.SendAsync(actualMessage.ToArray(), actualMessage.Count);
        }

        private async Task<string> Read()
        {
            var response = await udpClient.ReceiveAsync();
            return Encoding.ASCII.GetString(response.Buffer);
        }
    }
}