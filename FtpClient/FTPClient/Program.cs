using System;
using System.Threading.Tasks;

namespace FTPClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //await RunFtp();
            await RunTftp();
        }

        private static async Task RunTftp()
        {
            TftpClient tftpClient = new TftpClient("localhost", 69);
            await tftpClient.Get();
        }

        private static async Task RunFtp()
        {
            FtpClient ftpClient = new FtpClient("localhost", 21);
            await ftpClient.Connect();
            await ftpClient.SetPassive();
            await ftpClient.ListContents();
            string retrContents = await ftpClient.Retrieve("joku.txt");
            await ftpClient.Disconnect();
        }
    }
}
