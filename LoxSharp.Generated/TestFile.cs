//using LoxSharp.Common.Parser;

//namespace LoxSharp.Grammar
//{
//    public interface Visitor<T>
//    {
//        T VisitBinaryExpr(Binary b);
//        T VisitGroupingExpr(Grouping g);
//        T VisitLiteralExpr(Literal l);
//        T VisitUnaryExpr(Unary u);
//    }

//    public abstract class Expr
//    {
//        public abstract T Accept<T>(Visitor<T> visitor);
//    }

//    public class Binary : Expr
//    {
//        public Expr Left { get; set; }
//        public Token Op { get; set; }
//        public Expr Right { get; set; }
//        public T Accept<T>(Visitor<T> visitor)
//        {
//            return visitor.VisitBinaryExpr(this);
//        }

//        public override T Accept<T>(Visitor<T> visitor)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}
