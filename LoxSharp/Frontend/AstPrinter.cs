//using System.Text;
//using LoxSharp.Grammar;

//namespace LoxSharp.Frontend
//{
//    public class AstPrinter : ExprVisitor<string>
//    {
//        public string Print(Expr expr)
//        {
//            return expr.Accept(this);
//        }

//        public string VisitAssignExpr(Assign a)
//        {
//            return $"{a.Name} <- {Print(a.Value)}";
//        }

//        public string VisitBinaryExpr(Binary b)
//        {
//            return Parenthesize(b.Op.Lexeme,
//                b.Left, b.Right);
//        }

//        public string VisitCallExpr(Call c)
//        {
//            return "";
//        }

//        public string VisitGroupingExpr(Grouping g)
//        {
//            return Parenthesize("group", g.Expression);
//        }

//        public string VisitLiteralExpr(Literal l)
//        {
//            if (l.Value == null)
//                return "nil";

//            return l.Value.ToString();
//        }

//        public string VisitLogicalExpr(Logical l)
//        {
//            throw new System.NotImplementedException();
//        }

//        public string VisitUnaryExpr(Unary u)
//        {
//            return Parenthesize(u.Op.Lexeme, u.Right);
//        }

//        public string VisitVariableExpr(Variable v)
//        {
//            return v.Name.Lexeme;
//        }

//        private string Parenthesize(string name, params Expr[] expressions)
//        {
//            var builder = new StringBuilder();

//            builder.Append("(").Append(name);

//            foreach (var expr in expressions)
//            {
//                builder.Append($" {expr.Accept(this)}");
//            }

//            builder.Append(")");
//            return builder.ToString();
//        }
//    }
//}
