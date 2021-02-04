using System.Text;
using LoxSharp.Grammar;

namespace LoxSharp.Parser
{
    public class AstPrinter : Visitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitBinaryExpr(Binary b)
        {
            return Parenthesize(b.Op.Lexeme,
                b.Left, b.Right);
        }

        public string VisitGroupingExpr(Grouping g)
        {
            return Parenthesize("group", g.Expression);
        }

        public string VisitLiteralExpr(Literal l)
        {
            if (l.Value == null)
                return "nil";

            return l.Value.ToString();
        }

        public string VisitUnaryExpr(Unary u)
        {
            return Parenthesize(u.Op.Lexeme, u.Right);
        }

        private string Parenthesize(string name, params Expr[] expressions)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name);

            foreach (var expr in expressions)
            {
                builder.Append($" {expr.Accept(this)}");
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}
