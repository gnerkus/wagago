namespace Wagago
{
  internal interface IExpr
  {
    public TR Accept<TR>(IVisitor<TR> visitor);

   internal interface IVisitor<out TR> {
      TR VisitAssignExpr(Assign expr);
      TR VisitBinaryExpr(Binary expr);
      TR VisitInvocationExpr(Invocation expr);
      TR VisitPropGetExpr(PropGet expr);
      TR VisitPropSetExpr(PropSet expr);
      TR VisitSuperExpr(Super expr);
      TR VisitGroupingExpr(Grouping expr);
      TR VisitLiteralExpr(Literal expr);
      TR VisitLogicalExpr(Logical expr);
      TR VisitThisExpr(This expr);
      TR VisitUnaryExpr(Unary expr);
      TR VisitVariableExpr(Variable expr);
    }
  }
 internal class Assign: IExpr
  {
    internal Assign(Token identifier, IExpr value)
    {
      Identifier = identifier;
      Value = value;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitAssignExpr(this);
    }

    public readonly Token Identifier;
    public readonly IExpr Value;
  }
 internal class Binary: IExpr
  {
    internal Binary(IExpr left, Token operatr, IExpr right)
    {
      Left = left;
      Operatr = operatr;
      Right = right;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitBinaryExpr(this);
    }

    public readonly IExpr Left;
    public readonly Token Operatr;
    public readonly IExpr Right;
  }
 internal class Invocation: IExpr
  {
    internal Invocation(IExpr callee, Token paren, List<IExpr> arguments)
    {
      Callee = callee;
      Paren = paren;
      Arguments = arguments;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitInvocationExpr(this);
    }

    public readonly IExpr Callee;
    public readonly Token Paren;
    public readonly List<IExpr> Arguments;
  }
 internal class PropGet: IExpr
  {
    internal PropGet(IExpr owner, Token name)
    {
      Owner = owner;
      Name = name;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitPropGetExpr(this);
    }

    public readonly IExpr Owner;
    public readonly Token Name;
  }
 internal class PropSet: IExpr
  {
    internal PropSet(IExpr owner, Token name, IExpr value)
    {
      Owner = owner;
      Name = name;
      Value = value;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitPropSetExpr(this);
    }

    public readonly IExpr Owner;
    public readonly Token Name;
    public readonly IExpr Value;
  }
 internal class Super: IExpr
  {
    internal Super(Token keyword, Token method)
    {
      Keyword = keyword;
      Method = method;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitSuperExpr(this);
    }

    public readonly Token Keyword;
    public readonly Token Method;
  }
 internal class Grouping: IExpr
  {
    internal Grouping(IExpr expression)
    {
      Expression = expression;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitGroupingExpr(this);
    }

    public readonly IExpr Expression;
  }
 internal class Literal: IExpr
  {
    internal Literal(object value)
    {
      Value = value;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitLiteralExpr(this);
    }

    public readonly object Value;
  }
 internal class Logical: IExpr
  {
    internal Logical(IExpr left, Token operatr, IExpr right)
    {
      Left = left;
      Operatr = operatr;
      Right = right;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitLogicalExpr(this);
    }

    public readonly IExpr Left;
    public readonly Token Operatr;
    public readonly IExpr Right;
  }
 internal class This: IExpr
  {
    internal This(Token keyword)
    {
      Keyword = keyword;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitThisExpr(this);
    }

    public readonly Token Keyword;
  }
 internal class Unary: IExpr
  {
    internal Unary(Token operatr, IExpr right)
    {
      Operatr = operatr;
      Right = right;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitUnaryExpr(this);
    }

    public readonly Token Operatr;
    public readonly IExpr Right;
  }
 internal class Variable: IExpr
  {
    internal Variable(Token name)
    {
      Name = name;
    }

    public TR Accept<TR>(IExpr.IVisitor<TR> visitor)
    {
      return visitor.VisitVariableExpr(this);
    }

    public readonly Token Name;
  }
}
