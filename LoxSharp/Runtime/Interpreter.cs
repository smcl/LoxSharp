using System;
using System.Collections.Generic;
using System.Text;
using LoxSharp.Grammar;
using LoxSharp.Common.Parser;
using LoxSharp.StdLib;
using System.IO;

namespace LoxSharp.Runtime
{
    public class Interpreter : ExprVisitor<object>, StmtVisitor<object>
    {
        private readonly Environment _globals;
        private Environment _environment;
        private readonly IDictionary<Expr, int> _locals;
        private readonly Lox _lox;

        public readonly TextWriter StdOut;

        public Interpreter(TextWriter stdOut, Lox lox)
        {
            _globals = new Environment();
            _environment = _globals;
            _locals = new Dictionary<Expr, int>();

            _globals.Define("clock", new Clock());
            _globals.Define("add", new LoxCallable(2, (args) => (double)args[0] + (double)args[1]));
            _globals.Define("print", new PrintFunction());

            StdOut = stdOut;
            _lox = lox;
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
                _lox.RuntimeError(error);
            }
        }
        
        public void ExecuteBlock(IList<Stmt> statements, Environment environment)
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

        private static bool IsTruthy(object obj)
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

        private static bool IsEqual(object left, object right)
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

        private static void AssertNumberOperand(Token op, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new RuntimeError(op, "Operand must be a number");
        }

        private static void AssertNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double)
            {
                return;
            }

            throw new RuntimeError(op, "Operands must be numbers");
        }

        private static string Stringify(object obj)
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

        private object LookupVariable(Token name, Expr expr)
        {
            if (_locals.TryGetValue(expr, out var distance))
            {
                return _environment.GetAt(distance, name.Lexeme);
            }

            return _globals.Get(name);
        }


        public void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }

        public object VisitAssignExpr(Assign expr)
        {
            var value = Evaluate(expr.Value);

            if (_locals.TryGetValue(expr, out var distance))
            {
                _environment.AssignAt(distance, expr.Name, value);
            }
            else
            {
                _globals.Assign(expr.Name, value);
            }

            return value;
        }

        public object VisitBinaryExpr(Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.MINUS:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    AssertNumberOperands(expr.Op, left, right);
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
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    AssertNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
            }

            return null;
        }

        public object VisitCallExpr(Call expr)
        {
            var callee = Evaluate(expr.Callee);
            var arguments = new List<object>();

            foreach (var argument in expr.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            var function = (ILoxCallable)callee;

            if (arguments.Count != function.Arity)
            {
                throw new RuntimeError(expr.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Logical expr)
        {
            var left = Evaluate(expr.Left);

            if (expr.Op.Type == TokenType.OR)
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

            return Evaluate(expr.Right);
        }

        public object VisitUnaryExpr(Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    AssertNumberOperand(expr.Op, right);
                    return -(double)right;
            }

            return null;
        }

        public object VisitVariableExpr(Variable expr)
        {
            return LookupVariable(expr.Name, expr);
        }

        public object VisitGetExpr(Get expr)
        {
            var obj = Evaluate(expr.LoxObject);

            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).Get(expr.Name);
            }

            throw new RuntimeError(expr.Name, "Only instances have properties");
        }

        public object VisitSetExpr(Set expr)
        {
            var obj = Evaluate(expr.LoxObject);

            if (!(obj is LoxInstance))
            {
                throw new RuntimeError(expr.Name, "Only instances have fields.");
            }

            var value = Evaluate(expr.Value);
            ((LoxInstance)obj).Set(expr.Name, value);
            return value;
        }

        public object VisitThisExpr(This expr)
        {
            return LookupVariable(expr.Keyword, expr);
        }

        public object VisitBlockStmt(Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public object VisitClassStmt(Class stmt)
        {
            _environment.Define(stmt.Name.Lexeme, null);

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in stmt.Methods)
            {
                var isInitializer = method.Name.Lexeme == "init";
                var function = new LoxFunction(method, _environment, isInitializer);
                methods.Add(method.Name.Lexeme, function);
            }

            var klass = new LoxClass(stmt.Name.Lexeme, methods);
            _environment.Assign(stmt.Name, klass);
            return null;
        }

        public object VisitExpressionStmt(Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        public object VisitFunctionStmt(Function stmt)
        {
            var function = new LoxFunction(stmt, _environment, false);
            _environment.Define(stmt.Name.Lexeme, function);
            return null;
        }

        public object VisitIfStmt(If stmt)
        {
            var condition = Evaluate(stmt.Condition);

            if (IsTruthy(condition))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return null;
        }

        public object VisitPrintStmt(Print stmt)
        {
            var value = Evaluate(stmt.Expr);
            StdOut.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Var stmt)
        {
            object value = null;

            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _environment.Define(stmt.Name.Lexeme, value);
            return null;
        }

        public object VisitWhileStmt(While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return null;
        }

        public object VisitExitStmt(Exit stmt)
        {
            var result = Evaluate(stmt.Expr);

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

        public object VisitReturnStmt(Return stmt)
        {
            object value = null;

            if (stmt.Value != null)
            {
                value = Evaluate(stmt.Value);
            }

            throw new ReturnValue(value);
        }
    }
}
