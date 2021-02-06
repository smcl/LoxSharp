using LoxSharp.Common.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoxSharp.Frontend
{
    class Scanner
    {
        private string _source;
        private readonly IList<Token> _tokens;
        private readonly IDictionary<string, TokenType> _keywords;

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        public Scanner(string source)
        {
            _source = source;
            _tokens = new List<Token>();
            _keywords = InitialiseKeywords();
        }

        private IDictionary<string, TokenType> InitialiseKeywords()
        {
            var keywords = new Dictionary<string, TokenType>();

            keywords["and"] = TokenType.AND;
            keywords["class"] = TokenType.CLASS;
            keywords["else"] = TokenType.ELSE;
            keywords["false"] = TokenType.FALSE;
            keywords["for"] = TokenType.FOR;
            keywords["fun"] = TokenType.FUN;
            keywords["if"] = TokenType.IF;
            keywords["nil"] = TokenType.NIL;
            keywords["or"] = TokenType.OR;
            keywords["print"] = TokenType.PRINT;
            keywords["return"] = TokenType.RETURN;
            keywords["super"] = TokenType.SUPER;
            keywords["this"] = TokenType.THIS;
            keywords["true"] = TokenType.TRUE;
            keywords["var"] = TokenType.VAR;
            keywords["while"] = TokenType.WHILE;

            return keywords;
        }

        public IList<Token> ScanTokens()
        {
            while(!AtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private bool AtEnd()
        {
            return _current >= _source.Length;
        }

        private void ScanToken()
        {
            var c = Advance();

            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !AtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    _line++;
                    break;

                case '"': String(); break;

                default:

                    if (IsDigit(c))
                    {
                        Number();
                    } 
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Program.Error(_line, "Unexpected character.");
                    }

                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }

            var text = GetSubstring(_start, _current);
            
            if (_keywords.TryGetValue(text, out var keywordToken))
            {
                AddToken(keywordToken);
            }
            else
            {
                AddToken(TokenType.IDENTIFIER);
            }
        }

        private bool IsAlpha(char c)
        {
            return
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void String()
        {
            while (Peek() != '"' && !AtEnd())
            {
                if (Peek() == '\n')
                {
                    _line++;
                }
                Advance();
            }

            Advance();

            var value = GetSubstring(_start + 1, _current - 1);
            AddToken(TokenType.STRING, value);
        }

        private bool Match(char expected)
        {
            if (AtEnd())
            {
                return false;
            }

            if (_source[_current] != expected)
            {
                return false;
            }

            _current++;
            return true;
        }

        private char Peek()
        {
            if (AtEnd())
            {
                return '\0';
            }

            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length)
            {
                return '\0';
            }

            return _source[_current + 1];
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void Number()
        {
            while (IsDigit(Peek()))
            {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }

            var value = GetSubstring(_start, _current);
            AddToken(TokenType.NUMBER,
                double.Parse(value));
        }

        private char Advance()
        {
            _current++;

            return _source[_current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = GetSubstring(_start, _current);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        private string GetSubstring(int from, int to)
        {
            var length = to - from;
            var text = _source.Substring(from, length);
            return text;
        }
    }
}
