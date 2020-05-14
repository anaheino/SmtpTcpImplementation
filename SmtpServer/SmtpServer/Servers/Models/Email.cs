namespace SmtpServer.Server
{
    /// <summary>
    /// Mail singleton has a list of these.
    /// </summary>
    public class Email
    {
        public string Sender { get; internal set; }
        public string Recipient { get; internal set; }
        public string Data { get; internal set; }
        public string Helo { get; internal set; }
        public string SenderRaw { get; internal set; }
        public string RecipientRaw { get; internal set; }
        public string Quit { get; internal set; }
        public string TextPlain { get; internal set; }
    }
}