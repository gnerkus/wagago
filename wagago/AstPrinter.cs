namespace Wagago
{
    /// <summary>
    ///     A kind of visitor for Wagago that prints the AST to the console
    /// </summary>
    public class AstPrinter : Expr.IVisitor<string>
    {
        

        string Expr.IVisitor<string>.VisitBinaryExpr(Binary expr)
        {
            return Parenthesize(expr.Operatr.lexeme, expr.Left, expr.Right);
        }

        string Expr.IVisitor<string>.VisitGroupingExpr(Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        string Expr.IVisitor<string>.VisitLiteralExpr(Literal expr)
        {
            return expr.Value.Equals(null) ? "nil" : expr.Value.ToString() ?? "nil";
        }

        string Expr.IVisitor<string>.VisitLogicalExpr(Logical expr)
        {
            throw new NotImplementedException();
        }

        string Expr.IVisitor<string>.VisitUnaryExpr(Unary expr)
        {
            return Parenthesize(expr.Operatr.lexeme, expr.Right);
        }

        string Expr.IVisitor<string>.VisitVariableExpr(Variable expr)
        {
            
            throw new NotImplementedException();
        }
        
        string Expr.IVisitor<string>.VisitAssignExpr(Assign expr)
        {
            throw new NotImplementedException();
        }

        internal string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        private string Parenthesize(string name, params Expr[] exprs)
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