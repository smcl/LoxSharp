using Microsoft.CodeAnalysis;

namespace LoxSharp.Generated
{
    [Generator]
    public class StatementGenerator : BaseGrammarGenerator, ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var statements = new string[]
            {
                "Block      : List<Stmt> Statements",
                "Class      : Token Name, IList<Function> Methods",
                "Expression : Expr Expr",
                "Function   : Token Name, IList<Token> Parameters, IList<Stmt> Body",
                "If         : Expr Condition, Stmt ThenBranch, Stmt ElseBranch",
                "Print      : Expr Expr",
                "Var        : Token Name, Expr Initializer",
                "While      : Expr Condition, Stmt Body",
                "Exit       : Expr Expr",
                "Return     : Token Keyword, Expr Value"
            };

            GenerateGrammar(context, "Stmt", statements);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            /* deliberately left blank */
        }
    }
}
