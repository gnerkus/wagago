namespace Wagago
{
  internal abstract class Expr
  {
    public abstract TR Accept<TR>(IVisitor<TR> visitor);
  }
 internal interface IVisitor<out TR> {
    TR VisitBinaryExpr(Binary expr);
    TR VisitGroupingExpr(Grouping expr);
    TR VisitLiteralExpr(Literal expr);
    TR VisitUnaryExpr(Unary expr);
  }
 internal class Binary: Expr
  {
    internal Binary(Expr left, Token operatr, Expr right)
    {
      _left = left;
      _operatr = operatr;
      _right = right;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitBinaryExpr(this);
    }

    private readonly Expr _left;
    private readonly Token _operatr;
    private readonly Expr _right;
  }

 internal class Grouping: Expr
  {
    private Grouping(Expr expression)
    {
      _expression = expression;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitGroupingExpr(this);
    }

    private readonly Expr _expression;
  }
 internal class Literal: Expr
  {
    private Literal(object value)
    {
      _value = value;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitLiteralExpr(this);
    }

    private readonly object _value;
  }
 internal class Unary: Expr
  {
    private Unary(Token operatr, Expr right)
    {
      _operatr = operatr;
      _right = right;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitUnaryExpr(this);
    }

    private readonly Token _operatr;
    private readonly Expr _right;
  }
}
