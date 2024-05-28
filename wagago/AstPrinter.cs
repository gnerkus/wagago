namespace Wagago
{
    public class AstPrinter : IVisitor<string>
    {
        private string Print(Expr expr)
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
            throw new NotImplementedException();
        }

        string IVisitor<string>.VisitGroupingExpr(Grouping expr)
        {
            throw new NotImplementedException();
        }

        string IVisitor<string>.VisitLiteralExpr(Literal expr)
        {
            throw new NotImplementedException();
        }

        string IVisitor<string>.VisitUnaryExpr(Unary expr)
        {
            throw new NotImplementedException();
        }
    }
}