using System;
using System.IO;

namespace LoxSharp
{
    class Program
    {
        private static Lox _lox = new Lox(Console.Out, Console.Error);
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;

        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage loxsharp [script]");
                return 64;
            }

            else if (args.Length == 0)
            {
                RunPrompt();
                return 0;
            }

            else
            {
                RunFile(args[0]);
                return 0;
            }
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                _hadError = false;

                Run(line);
            }
        }

        private static void RunFile(string fileName)
        {
            var programBytes = File.ReadAllBytes(fileName);
            var programString = System.Text.Encoding.UTF8.GetString(programBytes);

            Run(programString);

            if (_hadError)
            {
                System.Environment.Exit(65);
            }

            if (_hadRuntimeError)
            {
                System.Environment.Exit(70);
            }
        }

        private static void Run(string source)
        {
            _lox.Run(source);
        }
    }
}
