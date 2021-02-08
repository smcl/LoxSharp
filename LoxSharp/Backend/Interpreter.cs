using System;
using System.Collections.Generic;
using System.Text;
using LoxSharp.Grammar;
using LoxSharp.Common.Parser;

namespace LoxSharp.Backend
{
    public class Interpreter : ExprVisitor<object>, StmtVisitor<object>
    {
        private Environment _environment;

        public Interpreter()
        {
            _environment = new Environment();
        }
            
        public void Interpret(IList<Stmt> statements)
        { 
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch(RuntimeError error)
            {
                Program.RuntimeError(error);
            }
        }

        public object VisitBinaryExpr(Binary b)
        {
            var left = Evaluate(b.Left);
            var right = Evaluate(b.Right);

            switch (b.Op.Type)
            {
                case TokenType.MINUS:
                    AssertNumberOperands(b.Op, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    AssertNumberOperands(b.Op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    AssertNumberOperands(b.Op, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }
                    break;
                case TokenType.GREATER:
                    AssertNumberOperands(b.Op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    AssertNumberOperands(b.Op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    AssertNumberOperands(b.Op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    AssertNumberOperands(b.Op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
            }

            return null;
        }

        public object VisitGroupingExpr(Grouping g)
        {
            return Evaluate(g.Expression);
        }

        public object VisitLiteralExpr(Literal l)
        {
            return l.Value;
        }

        public object VisitUnaryExpr(Unary u)
        {
            var right = Evaluate(u.Right);
            
            switch (u.Op.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    AssertNumberOperand(u.Op, right);
                    return -(double)right;
            }

            return null;
        }

        private object Evaluate(Expr expr)
        {
            if (expr == null)
            {
                return null;
            }

            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is bool)
            {
                return (bool)obj;
            }

            return true;
        }

        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        private void AssertNumberOperand(Token op, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new RuntimeError(op, "Operand must be a number");
        }

        private void AssertNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double)
            {
                return;
            }

            throw new RuntimeError(op, "Operands must be numbers");
        }

        private string Stringify(object obj)
        {
            if (obj == null)
            {
                return "nil";
            }

            if (obj is double)
            {
                var text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        public object VisitExpressionStmt(Expression e)
        {
            Evaluate(e.Expr);
            return null;
        }

        public object VisitPrintStmt(Print p)
        {
            var value = Evaluate(p.Expr);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVariableExpr(Variable v)
        {
            return _environment.Get(v.Name);
        }

        public object VisitVarStmt(Var v)
        {
            object value = null;

            if (v.Initializer != null)
            {
                value = Evaluate(v.Initializer);
            }

            _environment.Define(v.Name.Lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Assign a)
        {
            var value = Evaluate(a.Value);
            _environment.Assign(a.Name, value);
            return value;
        }

        public object VisitBlockStmt(Block b)
        {
            ExecuteBlock(b.Statements, new Environment(_environment));
            return null;
        }

        private void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previous = _environment;

            try
            {
                _environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        public object VisitExitStmt(Exit e)
        {
            var result = Evaluate(e.Expr);

            try
            {
                var exitCode = Convert.ToInt32((double)result);
                System.Environment.Exit(exitCode);
            } 
            catch (Exception)
            {
                System.Environment.Exit(-1);
            }

            return null;
        }

        public object VisitIfStmt(If i)
        {
            var condition = Evaluate(i.Condition);

            if (IsTruthy(condition))
            {
                Execute(i.ThenBranch);
            }
            else if (i.ElseBranch != null)
            { 
                Execute(i.ElseBranch);
            }

            return null;
        }

        public object VisitLogicalExpr(Logical l)
        {
            var left = Evaluate(l.Left);

            if (l.Op.Type == TokenType.OR)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(l.Right);
        }

        public object VisitWhileStmt(While w)
        {
            while (IsTruthy(Evaluate(w.Condition)))
            {
                Execute(w.Body);
            }

            return null;
        }
    }
}
