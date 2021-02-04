using System;
using System.IO;
using LoxSharp.Common.Parser;
using LoxSharp.Grammar;
using LoxSharp.Parser;

namespace LoxSharp
{
    class Program
    {
        private static bool _hadError;

        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage loxsharp [script]");
                return 64;
            }

            else if (args.Length == 0)
            {
                // RunPrompt();
                PrintTestExpression();
                return 0;
            }

            else
            {
                RunFile(args[0]);
                return 0;
            }
        }

        private static void PrintTestExpression()
        {
            var expression = new Binary(
                new Unary(
                    new Token(TokenType.MINUS, "-", null, 1),
                    new Literal(123)),
                new Token(TokenType.STAR, "*", null, 1),
                new Grouping(
                    new Literal(45.67)));

            Console.WriteLine(new AstPrinter().Print(expression));
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

                Run(line);
            }
        }

        private static void RunFile(string fileName)
        {
            var fileBytes = File.ReadAllBytes(fileName);

            var fileString = System.Text.Encoding.UTF8.GetString(fileBytes);

            Run(fileString);

            if (_hadError)
            {
                Environment.Exit(65);
            }
        }

        private static void Run(string source)
        {
            var scanner = new Parser.Scanner(source);
            var tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}
