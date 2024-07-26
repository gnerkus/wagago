namespace Wagago
{
  internal abstract class Stmt
  {
    public abstract TR Accept<TR>(IVisitor<TR> visitor);

   internal interface IVisitor<out TR> {
      TR VisitBlockStmt(Block stmt);
      TR VisitExpressionStmt(Expression stmt);
      TR VisitPrintStmt(Print stmt);
      TR VisitVarStmt(Var stmt);
    }
  }
 internal class Block: Stmt
  {
    internal Block(List<Stmt> statements)
    {
      Statements = statements;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitBlockStmt(this);
    }

    public readonly List<Stmt> Statements;
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
 internal class Var: Stmt
  {
    internal Var(Token identifier, Expr initializer)
    {
      Identifier = identifier;
      Initializer = initializer;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitVarStmt(this);
    }

    public readonly Token Identifier;
    public readonly Expr Initializer;
  }
}
