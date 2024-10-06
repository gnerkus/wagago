namespace Wagago
{
  internal interface IStmt
  {
    public TR Accept<TR>(IVisitor<TR> visitor);

   internal interface IVisitor<out TR> {
      TR VisitBlockStmt(Block stmt);
      TR VisitClassStmt(Class stmt);
      TR VisitExpressionStmt(Expression stmt);
      TR VisitPrintStmt(Print stmt);
      TR VisitIfStmt(If stmt);
      TR VisitWhileStmt(While stmt);
      TR VisitVarStmt(Var stmt);
      TR VisitReturnStmt(Return stmt);
      TR VisitFuncStmt(Func stmt);
    }
  }
 internal class Block: IStmt
  {
    internal Block(List<IStmt> statements)
    {
      Statements = statements;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitBlockStmt(this);
    }

    public readonly List<IStmt> Statements;
  }
 internal class Class: IStmt
  {
    internal Class(Token name, Variable superClass, List<Func> methods)
    {
      Name = name;
      SuperClass = superClass;
      Methods = methods;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitClassStmt(this);
    }

    public readonly Token Name;
    public readonly Variable SuperClass;
    public readonly List<Func> Methods;
  }
 internal class Expression: IStmt
  {
    internal Expression(IExpr expressn)
    {
      Expressn = expressn;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitExpressionStmt(this);
    }

    public readonly IExpr Expressn;
  }
 internal class Print: IStmt
  {
    internal Print(IExpr expression)
    {
      Expression = expression;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitPrintStmt(this);
    }

    public readonly IExpr Expression;
  }
 internal class If: IStmt
  {
    internal If(IExpr condition, IStmt thenBranch, IStmt elseBranch)
    {
      Condition = condition;
      ThenBranch = thenBranch;
      ElseBranch = elseBranch;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitIfStmt(this);
    }

    public readonly IExpr Condition;
    public readonly IStmt ThenBranch;
    public readonly IStmt ElseBranch;
  }
 internal class While: IStmt
  {
    internal While(IExpr condition, IStmt body)
    {
      Condition = condition;
      Body = body;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitWhileStmt(this);
    }

    public readonly IExpr Condition;
    public readonly IStmt Body;
  }
 internal class Var: IStmt
  {
    internal Var(Token identifier, IExpr initializer)
    {
      Identifier = identifier;
      Initializer = initializer;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitVarStmt(this);
    }

    public readonly Token Identifier;
    public readonly IExpr Initializer;
  }
 internal class Return: IStmt
  {
    internal Return(Token keyword, IExpr value)
    {
      Keyword = keyword;
      Value = value;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitReturnStmt(this);
    }

    public readonly Token Keyword;
    public readonly IExpr Value;
  }
 internal class Func: IStmt
  {
    internal Func(Token name, List<Token> funcParams, List<IStmt> body)
    {
      Name = name;
      FuncParams = funcParams;
      Body = body;
    }

    public TR Accept<TR>(IStmt.IVisitor<TR> visitor)
    {
      return visitor.VisitFuncStmt(this);
    }

    public readonly Token Name;
    public readonly List<Token> FuncParams;
    public readonly List<IStmt> Body;
  }
}
