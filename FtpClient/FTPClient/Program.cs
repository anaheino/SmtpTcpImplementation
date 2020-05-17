using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FTPClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var tasks = new List<Task>()
            {
                Task.Run(async () =>
                {
                    await RunTftp();
                }),
                Task.Run(async () =>
                {
                    await RunFtp();
                }),
            };
            Task.WaitAll(tasks.ToArray());
        }

        private static async Task RunTftp()
        {
            TftpClient tftpClient = new TftpClient("127.0.0.1", 69);
            await tftpClient.GetTextFile("joku.txt");
        }

        private static async Task RunFtp()
        {
            FtpClient ftpClient = new FtpClient("localhost", 21);
            await ftpClient.Connect("hullumies", "hulluheina");
            await ftpClient.SetPassive();
            await ftpClient.ListContents();
            string retrContents = await ftpClient.Retrieve("joku.txt");
            await ftpClient.Disconnect();
        }
    }
}
