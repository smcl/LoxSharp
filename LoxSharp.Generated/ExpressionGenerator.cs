using Microsoft.CodeAnalysis;

namespace LoxSharp.Generated
{
    [Generator]
    public class ExpressionGenerator : BaseGrammarGenerator, ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var expressions = new string[] {
                "Assign   : Token Name, Expr Value",
                "Binary   : Expr Left, Token Op, Expr Right",
                "Call     : Expr Callee, Token Paren, IList<Expr> Arguments",
                "Grouping : Expr Expression",
                "Literal  : Object Value",
                "Logical  : Expr Left, Token Op, Expr Right",
                "Unary    : Token Op, Expr Right",
                "Variable : Token Name",
                "Get      : Expr LoxObject, Token Name",
                "Set      : Expr LoxObject, Token Name, Expr Value",
                "This     : Token Keyword"
            };

            GenerateGrammar(context, "Expr", expressions);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            /* deliberately left blank */
        }
    }
}
