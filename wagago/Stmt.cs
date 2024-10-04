namespace Wagago
{
  internal abstract class Stmt
  {
    public abstract TR Accept<TR>(IVisitor<TR> visitor);

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
 internal class Class: Stmt
  {
    internal Class(Token name, Variable superClass, List<Func> methods)
    {
      Name = name;
      SuperClass = superClass;
      Methods = methods;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitClassStmt(this);
    }

    public readonly Token Name;
    public readonly Variable SuperClass;
    public readonly List<Func> Methods;
  }
 internal class Expression: Stmt
  {
    internal Expression(IExpr expressn)
    {
      Expressn = expressn;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitExpressionStmt(this);
    }

    public readonly IExpr Expressn;
  }
 internal class Print: Stmt
  {
    internal Print(IExpr expression)
    {
      Expression = expression;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitPrintStmt(this);
    }

    public readonly IExpr Expression;
  }
 internal class If: Stmt
  {
    internal If(IExpr condition, Stmt thenBranch, Stmt elseBranch)
    {
      Condition = condition;
      ThenBranch = thenBranch;
      ElseBranch = elseBranch;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitIfStmt(this);
    }

    public readonly IExpr Condition;
    public readonly Stmt ThenBranch;
    public readonly Stmt ElseBranch;
  }
 internal class While: Stmt
  {
    internal While(IExpr condition, Stmt body)
    {
      Condition = condition;
      Body = body;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitWhileStmt(this);
    }

    public readonly IExpr Condition;
    public readonly Stmt Body;
  }
 internal class Var: Stmt
  {
    internal Var(Token identifier, IExpr initializer)
    {
      Identifier = identifier;
      Initializer = initializer;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitVarStmt(this);
    }

    public readonly Token Identifier;
    public readonly IExpr Initializer;
  }
 internal class Return: Stmt
  {
    internal Return(Token keyword, IExpr value)
    {
      Keyword = keyword;
      Value = value;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitReturnStmt(this);
    }

    public readonly Token Keyword;
    public readonly IExpr Value;
  }
 internal class Func: Stmt
  {
    internal Func(Token name, List<Token> funcParams, List<Stmt> body)
    {
      Name = name;
      FuncParams = funcParams;
      Body = body;
    }

    public override TR Accept<TR>(IVisitor<TR> visitor)
    {
      return visitor.VisitFuncStmt(this);
    }

    public readonly Token Name;
    public readonly List<Token> FuncParams;
    public readonly List<Stmt> Body;
  }
}
