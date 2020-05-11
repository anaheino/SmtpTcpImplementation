using System;
using System.Collections.Generic;
using System.Text;

namespace SmtpServer.Server
{
    public sealed class MailSingleton
    {
        private static readonly MailSingleton instance = new MailSingleton();

        public static List<Email> emails { get; set; }
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static MailSingleton()
        {
        }

        private MailSingleton()
        {
        }

        public static MailSingleton Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
