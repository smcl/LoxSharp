using System;
using System.Collections.Generic;
using System.Text;

namespace LoxSharp.Common.Parser
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public object Literal { get; set; }
        public int Line { get; set; }

        public Token(
            TokenType type, 
            string lexeme, 
            object literal,
            int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
