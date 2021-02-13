using System.Collections.Generic;
using LoxSharp.Grammar;
using LoxSharp.Common.Parser;
using LoxSharp.Extensions;

namespace LoxSharp.Runtime
{
    public class Resolver : ExprVisitor<object>, StmtVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<IDictionary<string, bool>> _scopes;
        private FunctionType _currentFunction;
        private ClassType _currentClass;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
            _scopes = new Stack<IDictionary<string, bool>>();
            _currentFunction = FunctionType.NONE;
            _currentClass = ClassType.NONE;
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

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (_scopes.IsEmpty())
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
            if (_scopes.IsEmpty())
            {
                return;
            }

            var scope = _scopes.Peek();
            scope[name.Lexeme] = true;
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
                if (_currentFunction == FunctionType.INITIALIZER)
                {
                    Program.Error(r.Keyword, "Can't return a value from an initializer.");
                }
                Resolve(r.Value);
            }
            return null;
        }

        public object VisitUnaryExpr(Unary u)
        {
            Resolve(u.Right);
            return null;
        }

        public object VisitVariableExpr(Variable v)
        {
            if (!_scopes.IsEmpty())
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

        public object VisitWhileStmt(While w)
        {
            Resolve(w.Condition);
            Resolve(w.Body);
            return null;
        }

        public object VisitClassStmt(Class c)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.CLASS;

            Declare(c.Name);
            Define(c.Name);

            BeginScope();
            _scopes.Peek().Add("this", true);

            foreach (var method in c.Methods)
            {
                var declaration = method.Name.Lexeme == "init"
                    ? FunctionType.INITIALIZER
                    : FunctionType.METHOD;

                ResolveFunction(method, declaration);
            }

            EndScope();
            _currentClass = enclosingClass;
            return null;
        }

        public object VisitGetExpr(Get g)
        {
            Resolve(g.LoxObject);
            return null;
        }

        public object VisitSetExpr(Set s)
        {
            Resolve(s.Value);
            Resolve(s.LoxObject);
            return null;
        }

        public object VisitThisExpr(This t)
        {
            if (_currentClass == ClassType.NONE)
            {
                Program.Error(t.Keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(t, t.Keyword);
            return null;
        }
    }
}
