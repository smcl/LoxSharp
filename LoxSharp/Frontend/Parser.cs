using LoxSharp.Grammar;
using LoxSharp.Common.Parser;
using System.Collections.Generic;
using System.Text;
using System;

namespace LoxSharp.Frontend
{
    public class Parser
    {
        private readonly IList<Token> _tokens;
        private int _current;

        public Parser(IList<Token> tokens)
        {
            _tokens = tokens;
            _current = 0;
        }

        public IList<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!AtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Expr Expression()
        {
            return Assignment();
        }


        private Expr Assignment()
        {
            var expr = Equality();

            if (Match(TokenType.EQUAL))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr is Variable)
                {
                    var name = ((Variable)expr).Name;
                    return new Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            } 
            catch(ParseError error)
            {
                Synchronize();
                return null;
            }
        }

        private Expr Equality()
        {
            var expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var op = Previous();
                var right = Comparison();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            var expr = Term();

            while(Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var op = Previous();
                var right = Term();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Term()
        {
            var expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                var op = Previous();
                var right = Factor();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Factor()
        {
            var expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                var op = Previous();
                var right = Unary();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var op = Previous();
                var right = Unary();
                return new Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE))
            {
                return new Literal(false);
            }

            if (Match(TokenType.TRUE))
            {
                return new Literal(true);
            }

            if (Match(TokenType.NIL))
            {
                return new Literal(null);
            }

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Literal(Previous().Literal);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression");
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (AtEnd())
            {
                return false;
            }

            return Peek().Type == type;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
                return Advance();

            throw new System.Exception(message);
        }

        private ParseError Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!AtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }

        private Token Advance()
        {
            if (!AtEnd())
                _current++;

            return Previous();
        }

        private bool AtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private Stmt Statement()
        {
            if (Match(TokenType.EXIT)) return ExitStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Block(Block());

            return ExpressionStatement();
        }

        private Stmt ExitStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Exit(value);
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Print(value);
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(TokenType.IDENTIFIER, "Expect variable name");
            Expr initializer = null;

            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Var(name, initializer);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Expression(expr);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !AtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }
    }

    public class ParseError: System.Exception
    {
        /* ??? */
    }
}
