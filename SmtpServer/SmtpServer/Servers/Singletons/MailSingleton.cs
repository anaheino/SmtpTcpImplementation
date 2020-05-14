using System;
using System.Collections.Generic;
using System.Text;

namespace SmtpServer.Server
{
    /// <summary>
    /// Singleton for simulating an inbox. Thread-safe implementation.
    /// </summary>
    public sealed class MailSingleton
    {
        private static readonly MailSingleton instance = new MailSingleton();

        public static List<Email> emails { get; set; }
        
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
