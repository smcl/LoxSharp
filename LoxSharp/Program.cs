﻿using System;
using System.IO;
using LoxSharp.Common.Parser;
using LoxSharp.Grammar;
using LoxSharp.Frontend;
using LoxSharp.Backend;

namespace LoxSharp
{
    class Program
    {
        private static Interpreter interpreter = new Interpreter();
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
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var statements = parser.Parse();

            if (_hadError)
            {
                return;
            }

            interpreter.Interpret(statements);
            // Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}