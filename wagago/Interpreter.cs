namespace Wagago
{
    public class Interpreter: IVisitor<object>
    {
        object IVisitor<object>.VisitBinaryExpr(Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            return expr.Operatr.GetTokenType() switch
            {
                TokenType.MINUS => (double)left - (double)right,
                TokenType.PLUS => EvaluatePlus(left, right),
                TokenType.SLASH => (double)left / (double)right,
                TokenType.STAR => (double)left * (double)right,
                TokenType.GREATER => (double)left > (double)right,
                TokenType.GREATER_EQUAL => (double)left >= (double)right,
                TokenType.LESS => (double)left < (double)right,
                TokenType.LESS_EQUAL => (double)left <= (double)right,
                TokenType.BANG_EQUAL => !Equals(left, right),
                TokenType.EQUAL_EQUAL => Equals(left, right)
            };
        }

        /// <summary>
        ///     Handles a + b
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static object EvaluatePlus(object left, object right)
        {
            if (left is string || right is string)
            {
                return (string)left + (string)right;
            }

            return (double)left + (double)right;
        }

        /// <summary>
        ///     Recursively evaluate the expression within the group (parentheses)
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        object IVisitor<object>.VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        object IVisitor<object>.VisitLiteralExpr(Literal expr)
        {
            return expr.Value;
        }

        /// <summary>
        ///     The operand of a unary expression must be evaluated first.
        ///     <para>We cast to Double before applying the operator</para>
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        object IVisitor<object>.VisitUnaryExpr(Unary expr)
        {
            var right = Evaluate(expr.Right);

            return expr.Operatr.GetTokenType() switch
            {
                TokenType.MINUS => -(double)right,
                TokenType.BANG => !IsTruthy(right)
            };
        }

        /// <summary>
        ///  Implement Ruby's truthy rule: only false and nil are falsey
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        private bool IsTruthy(object right)
        {
            if (right.Equals(null)) return false;
            // `is` operator checks if the run-time type of an expression result 
            // is compatible with a given type.
            if (right is bool b) return b;
            return true;
        }
    }
}