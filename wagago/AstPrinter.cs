namespace Wagago
{
    /// <summary>
    ///     A kind of visitor for Wagago that prints the AST to the console
    /// </summary>
    public class AstPrinter : IExpr.IVisitor<string>
    {
        string IExpr.IVisitor<string>.VisitBinaryExpr(Binary expr)
        {
            return Parenthesize(expr.Operatr.lexeme, expr.Left, expr.Right);
        }

        string IExpr.IVisitor<string>.VisitInvocationExpr(Invocation expr)
        {
            throw new NotImplementedException();
        }

        string IExpr.IVisitor<string>.VisitPropGetExpr(PropGet expr)
        {
            throw new NotImplementedException();
        }

        string IExpr.IVisitor<string>.VisitPropSetExpr(PropSet expr)
        {
            throw new NotImplementedException();
        }

        string IExpr.IVisitor<string>.VisitSuperExpr(Super expr)
        {
            throw new NotImplementedException();
        }

        string IExpr.IVisitor<string>.VisitGroupingExpr(Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        string IExpr.IVisitor<string>.VisitLiteralExpr(Literal expr)
        {
            return expr.Value.Equals(null) ? "nil" : expr.Value.ToString() ?? "nil";
        }

        string IExpr.IVisitor<string>.VisitLogicalExpr(Logical expr)
        {
            throw new NotImplementedException();
        }

        string IExpr.IVisitor<string>.VisitThisExpr(This expr)
        {
            throw new NotImplementedException();
        }

        string IExpr.IVisitor<string>.VisitUnaryExpr(Unary expr)
        {
            return Parenthesize(expr.Operatr.lexeme, expr.Right);
        }

        string IExpr.IVisitor<string>.VisitVariableExpr(Variable expr)
        {
            
            throw new NotImplementedException();
        }
        
        string IExpr.IVisitor<string>.VisitAssignExpr(Assign expr)
        {
            throw new NotImplementedException();
        }

        internal string Print(IExpr expr)
        {
            return expr.Accept(this);
        }

        private string Parenthesize(string name, params IExpr[] exprs)
        {
            var output = new List<string>
            {
                "(",
                name
            };


            foreach (var expr in exprs)
            {
                output.Add(" ");
                output.Add(expr.Accept(this));
            }

            output.Add(")");

            return string.Join("", output);
        }
    }
}