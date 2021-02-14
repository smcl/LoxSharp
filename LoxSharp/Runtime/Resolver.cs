using System.Collections.Generic;
using LoxSharp.Grammar;
using LoxSharp.Common.Parser;
using LoxSharp.Extensions;

namespace LoxSharp.Runtime
{
    public class Resolver : ExprVisitor<object>, StmtVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Lox _lox;
        private readonly Stack<IDictionary<string, bool>> _scopes;
        private FunctionType _currentFunction;
        private ClassType _currentClass;

        public Resolver(Interpreter interpreter, Lox lox)
        {
            _interpreter = interpreter;
            _lox = lox;
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
                _lox.Error(name, "Already variable with this name in this scope");
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

        public object VisitAssignExpr(Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitBinaryExpr(Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitCallExpr(Call expr)
        {
            Resolve(expr.Callee);

            foreach (var argument in expr.Arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitGroupingExpr(Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public object VisitLiteralExpr(Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitUnaryExpr(Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Variable expr)
        {
            if (!_scopes.IsEmpty())
            {
                var scope = _scopes.Peek();
                if (scope.TryGetValue(expr.Name.Lexeme, out var res))
                {
                    if (!res)
                    {
                        _lox.Error(expr.Name, "Can't read local variable in its own initializer");
                    }
                }
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitGetExpr(Get expr)
        {
            Resolve(expr.LoxObject);
            return null;
        }

        public object VisitSetExpr(Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.LoxObject);
            return null;
        }

        public object VisitThisExpr(This expr)
        {
            if (_currentClass == ClassType.NONE)
            {
                _lox.Error(expr.Keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitBlockStmt(Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object VisitClassStmt(Class stmt)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.CLASS;

            Declare(stmt.Name);
            Define(stmt.Name);

            BeginScope();
            _scopes.Peek().Add("this", true);

            foreach (var method in stmt.Methods)
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

        public object VisitExpressionStmt(Expression stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        public object VisitFunctionStmt(Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            ResolveFunction(stmt, FunctionType.FUNCTION);
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

        public object VisitPrintStmt(Print stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        public object VisitVarStmt(Var stmt)
        {
            Declare(stmt.Name);

            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }

            Define(stmt.Name);
            return null;
        }

        public object VisitWhileStmt(While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        public object VisitExitStmt(Exit stmt)
        {
            return null;
        }

        public object VisitReturnStmt(Return stmt)
        {
            if (_currentFunction == FunctionType.NONE)
            {
                _lox.Error(stmt.Keyword, "Can't return from top-level code.");
            }

            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.INITIALIZER)
                {
                    _lox.Error(stmt.Keyword, "Can't return a value from an initializer.");
                }
                Resolve(stmt.Value);
            }
            return null;
        }
    }
}
