namespace Wagago
{
  internal abstract class Expr
  {
    public abstract TR Accept<TR>(IVisitor<TR> visitor);

   internal interface IVisitor<out TR> {
      TR VisitAssignExpr(Assign expr);
      TR VisitBinaryExpr(Binary expr);
      TR VisitInvocationExpr(Invocation expr);
      TR VisitGroupingExpr(Grouping expr);
      TR VisitLiteralExpr(Literal expr);
      TR VisitLogicalExpr(Logical expr);
      TR VisitUnaryExpr(Unary expr);
      TR VisitVariableExpr(Variable expr);
    }
  }
 internal class Assign: Expr
  {
    internal Assign(Token identifier, Expr value)
    {
      Identifier = identifier;
      Value = value;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitAssignExpr(this);
    }

    public readonly Token Identifier;
    public readonly Expr Value;
  }
 internal class Binary: Expr
  {
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

    public readonly Expr Left;
    public readonly Token Operatr;
    public readonly Expr Right;
  }
 internal class Invocation: Expr
  {
    internal Invocation(Expr callee, Token paren, List<Expr> arguments)
    {
      Callee = callee;
      Paren = paren;
      Arguments = arguments;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitInvocationExpr(this);
    }

    public readonly Expr Callee;
    public readonly Token Paren;
    public readonly List<Expr> Arguments;
  }
 internal class Grouping: Expr
  {
    internal Grouping(Expr expression)
    {
      Expression = expression;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitGroupingExpr(this);
    }

    public readonly Expr Expression;
  }
 internal class Literal: Expr
  {
    internal Literal(object value)
    {
      Value = value;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitLiteralExpr(this);
    }

    public readonly object Value;
  }
 internal class Logical: Expr
  {
    internal Logical(Expr left, Token operatr, Expr right)
    {
      Left = left;
      Operatr = operatr;
      Right = right;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitLogicalExpr(this);
    }

    public readonly Expr Left;
    public readonly Token Operatr;
    public readonly Expr Right;
  }
 internal class Unary: Expr
  {
    internal Unary(Token operatr, Expr right)
    {
      Operatr = operatr;
      Right = right;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitUnaryExpr(this);
    }

    public readonly Token Operatr;
    public readonly Expr Right;
  }
 internal class Variable: Expr
  {
    internal Variable(Token name)
    {
      Name = name;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitVariableExpr(this);
    }

    public readonly Token Name;
  }
}
