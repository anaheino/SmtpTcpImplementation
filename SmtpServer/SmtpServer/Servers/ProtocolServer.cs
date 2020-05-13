using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SmtpServer.Server
{
    public class ProtocolServer
    {
        private const int byteSize = 8192;

        internal TcpClient tcpClient { get; set; }
        internal NetworkStream stream { get; set; }

        internal void Init(TcpClient client)
        {
            tcpClient = client;
            stream = tcpClient.GetStream();
        }

        internal async Task Write(string strMessage)
        {
            var networkStream = tcpClient.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(strMessage + "\r\n");
            await networkStream.WriteAsync(buffer, 0, buffer.Length);
            await networkStream.FlushAsync();
        }

        internal async Task<string> Read()
        {
            byte[] messageBytes = new byte[byteSize];
            int bytesRead = 0;
            ASCIIEncoding encoder = new ASCIIEncoding();
            bytesRead = await stream.ReadAsync(messageBytes, 0, byteSize);
            string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
            return strMessage;
        }

    }
}
