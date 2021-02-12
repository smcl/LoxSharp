using System;
using System.Collections.Generic;
using System.Text;
using LoxSharp.Grammar;
using LoxSharp.Common.Parser;
using LoxSharp.StdLib;

namespace LoxSharp.Backend
{
    public class Interpreter : ExprVisitor<object>, StmtVisitor<object>
    {
        public Environment Globals;
        private Environment _environment;
        private readonly IDictionary<Expr, int> _locals;

        public Interpreter()
        {
            Globals = new Environment();
            _environment = Globals;
            _locals = new Dictionary<Expr, int>();

            Globals.Define("clock", new Clock());
            Globals.Define("add", new LoxCallable(2, (args) => (double)args[0] + (double)args[1]));
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
            return LookupVariable(v.Name, v);
        }

        private object LookupVariable(Token name, Expr v)
        {
            if (_locals.TryGetValue(v, out var distance))
            {
                return _environment.GetAt(distance, name.Lexeme);
            }

            // OK so this is likely WRONG. The proper return is:
            //
            //   return Globals.Get(name);
            //
            // For some reason "this" doesn't end up in _locals
            // even though it's in the _environment. I need to
            // find out why it doesn't get set there. I will
            // probably need to work through that annoying bit
            // on Resolving & Binding :-/

            return _environment.Get(name);
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

            if (_locals.TryGetValue(a, out var distance))
            {
                _environment.AssignAt(distance, a.Name, value);
            }
            else 
            {
                Globals.Assign(a.Name, value);
            }

            return value;
        }

        public object VisitBlockStmt(Block b)
        {
            ExecuteBlock(b.Statements, new Environment(_environment));
            return null;
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

        public object VisitCallExpr(Call c)
        {
            var callee = Evaluate(c.Callee);
            var arguments = new List<object>();

            foreach (var argument in c.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            var function = (ILoxCallable)callee;

            if (arguments.Count != function.Arity)
            {
                throw new RuntimeError(c.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitFunctionStmt(Function f)
        {
            var function = new LoxFunction(f, _environment, false);
            _environment.Define(f.Name.Lexeme, function);
            return null;
        }

        public object VisitReturnStmt(Return r)
        {
            object value = null;

            if (r.Value != null)
            {
                value = Evaluate(r.Value);
            }

            throw new ReturnValue(value);
        }

        public void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }

        public object VisitClassStmt(Class c)
        {
            _environment.Define(c.Name.Lexeme, null);

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in c.Methods)
            {
                var isInitializer = method.Name.Lexeme == "init";
                var function = new LoxFunction(method, _environment, isInitializer);
                methods.Add(method.Name.Lexeme, function);
            }

            var klass = new LoxClass(c.Name.Lexeme, methods);
            _environment.Assign(c.Name, klass);
            return null;
        }

        public object VisitGetExpr(Get g)
        {
            var obj = Evaluate(g.LoxObject);

            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).Get(g.Name);
            }

            throw new RuntimeError(g.Name, "Only instances have properties");
        }

        public object VisitSetExpr(Set s)
        {
            var obj = Evaluate(s.LoxObject);

            if (!(obj is LoxInstance))
            {
                throw new RuntimeError(s.Name, "Only instances have fields.");
            }

            var value = Evaluate(s.Value);
            ((LoxInstance)obj).Set(s.Name, value);
            return value;
        }

        public object VisitThisExpr(This t)
        {
            return LookupVariable(t.Keyword, t);
        }
    }
}
