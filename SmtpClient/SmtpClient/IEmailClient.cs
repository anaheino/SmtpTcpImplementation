using System.Threading.Tasks;

namespace SmtpClient
{
    internal interface IEmailClient
    {
        Task Connect(bool ssl = true);
        Task<bool> Login();
        Task<string> OpenInbox(int index = 0);
        Task<string> Disconnect();
    }
}