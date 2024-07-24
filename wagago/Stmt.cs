namespace Wagago
{
  internal abstract class Stmt
  {
    public abstract TR Accept<TR>(IVisitor<TR> visitor);

   internal interface IVisitor<out TR> {
      TR VisitExpressionStmt(Expression stmt);
      TR VisitPrintStmt(Print stmt);
    }
  }
 internal class Expression: Stmt
  {
    internal Expression(Expr expressn)
    {
      Expressn = expressn;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitExpressionStmt(this);
    }

    public readonly Expr Expressn;
  }
 internal class Print: Stmt
  {
    internal Print(Expr expression)
    {
      Expression = expression;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitPrintStmt(this);
    }

    public readonly Expr Expression;
  }
}
