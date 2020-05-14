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
            await ftpClient.Retrieve("C:\\joku.txt");
            await ftpClient.Disconnect();
        }
    }
}
