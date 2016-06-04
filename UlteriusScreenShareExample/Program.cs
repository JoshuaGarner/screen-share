using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UlteriusScreenShare;

namespace UlteriusScreenShareExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server(IPAddress.Any, 5555);
            server.Start();
            Console.Read();
        }
    }
}
