using System;
using System.Threading;

namespace Bis
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (Communicator communicator = new Communicator("127.0.0.1"))
                {
                    string message = "Hello!";
                    if(args.Length > 0)
                    {
                        message = string.Join(' ', args);
                    }
                    communicator.Open();
                    string response = communicator.Send(message);
                    Console.WriteLine($"{response}");
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
    }
}
