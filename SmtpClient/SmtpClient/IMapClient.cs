namespace SmtpClient
{
    internal class IMapClient : IEmailClient
    {
        private string username;
        private string password;
        private string server;
        public static readonly int IMAP4_SSL = 993;
        public static readonly int IMAP4 = 143;

        public IMapClient(string username, string password, string server)
        {
            this.username = username;
            this.password = password;
            this.server = server;
        }

        public void Connect(bool ssl = true)
        {
            throw new System.NotImplementedException();
        }

        public string Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public bool Login()
        {
            throw new System.NotImplementedException();
        }

        public string OpenInbox(int index = 0)
        {
            throw new System.NotImplementedException();
        }
    }
}