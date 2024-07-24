namespace Wagago
{
    internal abstract class Expr
    {
        public abstract TR Accept<TR>(IVisitor<TR> visitor);

        internal interface IVisitor<out TR>
        {
            TR VisitBinaryExpr(Binary expr);
            TR VisitGroupingExpr(Grouping expr);
            TR VisitLiteralExpr(Literal expr);
            TR VisitUnaryExpr(Unary expr);
        }
    }

    internal class Binary : Expr
    {
        public readonly Expr Left;
        public readonly Token Operatr;
        public readonly Expr Right;

        internal Binary(Expr left, Token operatr, Expr right)
        {
            Left = left;
            Operatr = operatr;
            Right = right;
        }

        public override TR Accept<TR>(IVisitor<TR> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    internal class Grouping : Expr
    {
        public readonly Expr Expression;

        internal Grouping(Expr expression)
        {
            Expression = expression;
        }

        public override TR Accept<TR>(IVisitor<TR> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }

    internal class Literal : Expr
    {
        public readonly object Value;

        internal Literal(object value)
        {
            Value = value;
        }

        public override TR Accept<TR>(IVisitor<TR> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    internal class Unary : Expr
    {
        public readonly Token Operatr;
        public readonly Expr Right;

        internal Unary(Token operatr, Expr right)
        {
            Operatr = operatr;
            Right = right;
        }

        public override TR Accept<TR>(IVisitor<TR> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }
}