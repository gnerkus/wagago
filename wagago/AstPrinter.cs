namespace Wagago
{
    /// <summary>
    /// A kind of visitor for Wagago that prints the AST to the console
    /// </summary>
    public class AstPrinter : IVisitor<string>
    {
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

        string IVisitor<string>.VisitBinaryExpr(Binary expr)
        {
            return Parenthesize(expr.Operatr.lexeme, expr.Left, expr.Right);
        }

        string IVisitor<string>.VisitGroupingExpr(Grouping expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        string IVisitor<string>.VisitLiteralExpr(Literal expr)
        {
            return expr.Value == null ? "nil" : expr.Value.ToString();
        }

        string IVisitor<string>.VisitUnaryExpr(Unary expr)
        {
            return Parenthesize(expr.Operatr.lexeme, expr.Right);
        }
    }
}