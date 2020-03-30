using BisCore;
using System;
using System.Threading.Tasks;

namespace BrownieInteractionService
{
    class Program
    {
        static void Main(string[] args)
        {

            CommandReceiver server = new CommandReceiver();

            server.Open();
            Console.WriteLine($"Started: port {server.Port}");

            Console.ReadKey();
            server.Close();
            Console.WriteLine("Closed.");
        }
    }
}
