using System;
using System.Collections.Generic;
using System.Text;
using LoxSharp.Grammar;
using LoxSharp.Common.Parser;
using LoxSharp.StdLib;
using LoxSharp.Extensions;

namespace LoxSharp.Backend
{
    public class Resolver : ExprVisitor<object>, StmtVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<IDictionary<string, bool>> _scopes;
        private readonly IDictionary<Expr, int> _locals;
        private FunctionType _currentFunction;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
            _scopes = new Stack<IDictionary<string, bool>>();
            _locals = new Dictionary<Expr, int>();
            _currentFunction = FunctionType.NONE;
        }

        public object VisitAssignExpr(Assign a)
        {
            Resolve(a.Value);
            ResolveLocal(a, a.Name);
            return null;
        }

        public object VisitBinaryExpr(Binary b)
        {
            Resolve(b.Left);
            Resolve(b.Right);
            return null;
        }

        public object VisitBlockStmt(Block b)
        {
            BeginScope();
            Resolve(b.Statements);
            EndScope();
            return null;
        }

        public object VisitCallExpr(Call c)
        {
            Resolve(c.Callee);

            foreach (var argument in c.Arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitExitStmt(Exit e)
        {
            return null;
        }

        public object VisitExpressionStmt(Expression e)
        {
            Resolve(e.Expr);
            return null;
        }

        public object VisitFunctionStmt(Function f)
        {
            Declare(f.Name);
            Define(f.Name);
            ResolveFunction(f, FunctionType.FUNCTION);
            return null;
        }

        private void ResolveFunction(Function f, FunctionType type)
        {
            var enclosingFunction = _currentFunction;
            _currentFunction = type;

            BeginScope();

            foreach (var p in f.Parameters)
            {
                Declare(p);
                Define(p);
            }

            Resolve(f.Body);
            EndScope();

            _currentFunction = enclosingFunction;
        }

        public object VisitGroupingExpr(Grouping g)
        {
            Resolve(g.Expression);
            return null;
        }

        public object VisitIfStmt(If i)
        {
            Resolve(i.Condition);
            Resolve(i.ThenBranch);
            if (i.ElseBranch != null)
            {
                Resolve(i.ElseBranch);
            }
            return null;
        }

        public object VisitLiteralExpr(Literal l)
        {
            return null;
        }

        public object VisitLogicalExpr(Logical l)
        {
            Resolve(l.Left);
            Resolve(l.Right);
            return null;
        }

        public object VisitPrintStmt(Print p)
        {
            Resolve(p.Expr);
            return null;
        }

        public object VisitReturnStmt(Return r)
        {
            if (_currentFunction == FunctionType.NONE)
            {
                Program.Error(r.Keyword, "Can't return from top-level code.");
            }

            if (r.Value != null)
            {
                Resolve(r.Value);
            }
            return null;
        }

        public object VisitUnaryExpr(Unary u)
        {
            throw new NotImplementedException();
        }

        public object VisitVariableExpr(Variable v)
        {
            if (_scopes.Count > 0)
            {
                var scope = _scopes.Peek();
                if (scope.TryGetValue(v.Name.Lexeme, out var res))
                {
                    if (!res)
                    {
                        Program.Error(v.Name, "Can't read local variable in its own initializer");
                    }
                }
            }

            ResolveLocal(v, v.Name);
            return null;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (var i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes.Get(i);

                if (scope.ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                    return;
                }
            }
        }

        public object VisitVarStmt(Var v)
        {
            Declare(v.Name);

            if (v.Initializer != null)
            {
                Resolve(v.Initializer);
            }
             
            Define(v.Name);
            return null;
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            var scope = _scopes.Peek();

            if (scope.ContainsKey(name.Lexeme))
            {
                Program.Error(name, "Already variable with this name in this scope");
            }

            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            scope[name.Lexeme] = true;
            // scope.Add(name.Lexeme, true);
        }

        public object VisitWhileStmt(While w)
        {
            Resolve(w.Condition);
            Resolve(w.Body);
            return null;
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        public void Resolve(IList<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }
    }
}
