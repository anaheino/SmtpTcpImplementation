using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FTPClient
{
    internal class TftpClient
    {
        private IPEndPoint serverEndpoint;
        private EndPoint dataEndpoint;
        private Socket tftpSocket;

        public enum Opcodes
        {
            Unknown = 0,
            Read = 1,
            Write = 2,
            Data = 3,
            Ack = 4,
            Error = 5
        }
        public enum Modes
        {
            Unknown = 0,
            NetAscii = 1,
            Octet = 2,
            Mail = 3
        }

        public TftpClient(string address, int port)
        {
            IPHostEntry host = Dns.GetHostEntry(address);
            serverEndpoint = new IPEndPoint(host.AddressList[0], port);
            dataEndpoint = serverEndpoint;
            serverEndpoint.Port = ((IPEndPoint)dataEndpoint).Port;
        }

        internal async Task GetTextFile(string filePath)
        {
            // Temp solution with partly hardcoded path, in my case I wanted to store everything received to C:/dev/ -folder.
            BinaryWriter fileStream = new BinaryWriter(new FileStream("C:/dev/" + filePath, FileMode.Create, FileAccess.Write, FileShare.Read));
            tftpSocket = new Socket(serverEndpoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = 3000
            };
            byte[] sendBuffer = CreateRequest(Opcodes.Read, filePath, Modes.Octet);
            byte[] receiveBuffer = new byte[516];

            tftpSocket.SendTo(sendBuffer, sendBuffer.Length, SocketFlags.None, serverEndpoint);
            
            int messageLength = tftpSocket.ReceiveFrom(receiveBuffer, ref dataEndpoint);
            int packetNumber = 1;
            bool continueReceiving = true;
            while (continueReceiving)
            {
                if (((Opcodes)receiveBuffer[1]) == Opcodes.Error)
                {
                    //If we get here, something was messed up.
                    fileStream.Close();
                    tftpSocket.Close();
                    continueReceiving = false;
                }
                // If packet number is correct, this is the thing to save to a file.
                // Some extra handling would be necessary for longer files, but for excercise purposes, this will do fine.
                if (receiveBuffer[3] == packetNumber)
                {
                    // Writing everything to file, except the headers. So data only. 
                    // So here we receive the data to be written to the file.
                    fileStream.Write(receiveBuffer, 4, messageLength - 4);
                    sendBuffer = CreateAckPacket(packetNumber++);
                    await tftpSocket.SendToAsync(sendBuffer, SocketFlags.None, serverEndpoint);
                }
                if (messageLength < 516)
                {
                    // This means it was the final packet
                    continueReceiving = false;
                }
                else
                {
                    messageLength = (await tftpSocket.ReceiveFromAsync(receiveBuffer, SocketFlags.None, dataEndpoint)).ReceivedBytes;
                }
            }
            tftpSocket.Close();
            fileStream.Close();
        }

        private byte[] CreateAckPacket(int blockNumber)
        {
            byte[] ret = new byte[4];

            // Set type as Ack and set return number.
            ret[0] = 0;
            ret[1] = (byte)Opcodes.Ack;
            ret[2] = (byte)((blockNumber >> 8) & 0xff);
            ret[3] = (byte)(blockNumber & 0xff);
            return ret;
        }

        private byte[] CreateRequest(Opcodes opCode, string remoteFile, Modes tftpMode)
        {
            int index = 0;
            string mode = tftpMode.ToString().ToLowerInvariant();
            byte[] ret = new byte[mode.Length + remoteFile.Length + 4];
            
            //Setting the first bytes to be req type indentifier
            ret[index++] = 0;
            ret[index++] = (byte)opCode;
            // Setting the index to skip forward the amount of bytes the filename has converted to integer,
            // effectively storing filename in the buffer.
            index += Encoding.ASCII.GetBytes(remoteFile, 0, remoteFile.Length, ret, index);
            // This signals the end of the filename and data section
            ret[index++] = 0;
            index += Encoding.ASCII.GetBytes(mode, 0, mode.Length, ret, index);
            // This signals the end of the mode section
            ret[index] = 0;
            return ret;
        }

    }
}