using System.IO;
using LoxSharp.Common.Parser;
using LoxSharp.Frontend;
using LoxSharp.Runtime;

namespace LoxSharp
{
    public class Lox
    {
        private readonly Interpreter _interpreter;
        
        public bool _hadError = false;
        public bool _hadRuntimeError = false;

        public readonly TextWriter StdOut;
        public readonly TextWriter StdErr;

        public Lox(TextWriter stdout, TextWriter stderr)
        {
            StdOut = stdout;
            StdErr = stderr;
            _interpreter = new Interpreter(StdOut, this);  // BIG oof here
        }

        public void Run(string source)
        {
            var scanner = new Scanner(source, this);
            var tokens = scanner.ScanTokens();

            if (_hadError)
            {
                return;
            }

            var parser = new Parser(tokens, this);
            var statements = parser.Parse();

            if (_hadError)
            {
                return;
            }

            var resolver = new Resolver(_interpreter, this);
            resolver.Resolve(statements);

            if (_hadError)
            {
                return;
            }

            _interpreter.Interpret(statements);
        }

        public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public void Error(Token token, string message)
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

        public void RuntimeError(RuntimeError error)
        {
            StdErr.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
            _hadRuntimeError = true;
        }

        private void Report(int line, string where, string message)
        {
            StdErr.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}
