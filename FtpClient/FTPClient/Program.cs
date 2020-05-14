using System;
using System.Threading.Tasks;

namespace FTPClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            FtpClient ftpClient = new FtpClient("localhost", 21, 100);
            await ftpClient.Connect();
            await ftpClient.SetPassive();
            await ftpClient.ListContents();
            string retrContents = await ftpClient.Retrieve("joku.txt");
            await ftpClient.Disconnect();
        }
    }
}
