using System;
using System.Threading.Tasks;

namespace Yali.Examples
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.Title = "Yali Examples";
            Console.CursorVisible = false;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Select your choice:");
                Console.WriteLine(" [1] - Proxy example");
                Console.WriteLine(" [q] - Quit");
                Console.WriteLine();

                var key = Console.ReadKey().Key;
                Console.Clear();

                switch (key)
                {
                    case ConsoleKey.D1:
                        await _01._Proxy.Example.Run();
                        break;
                    case ConsoleKey.Q:
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }

                Console.WriteLine();
                Console.Write("Press any key to continue.");
                Console.ReadKey();
            }
        }
    }
}
