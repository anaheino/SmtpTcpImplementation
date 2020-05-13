using System.Threading.Tasks;

namespace SmtpServer.Server
{
    public interface IMailServer
    {
        Task Run();
    }
}