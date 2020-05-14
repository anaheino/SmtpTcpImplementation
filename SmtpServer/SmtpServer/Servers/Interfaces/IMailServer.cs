using System.Net.Sockets;
using System.Threading.Tasks;

namespace SmtpServer.Server
{
    /// <summary>
    /// Mail server interface.
    /// </summary>
    public interface IMailServer
    {
        Task Run();
        void Init(TcpClient client);

    }
}