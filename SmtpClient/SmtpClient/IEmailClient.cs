namespace SmtpClient
{
    internal interface IEmailClient
    {
        void Connect(bool ssl = true);
        bool Login();
        string OpenInbox(int index = 0);
        string Disconnect();
    }
}