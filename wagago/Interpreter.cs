namespace Wagago
{
    public class Interpreter: Expr.IVisitor<object>
    {
        object Expr.IVisitor<object>.VisitBinaryExpr(Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Operatr.GetTokenType())
            {
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Operatr, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    return EvaluatePlus(expr.Operatr, left, right);
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Operatr, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Operatr, left, right);
                    return (double)left * (double)right;
                case TokenType.GREATER:
                    CheckNumberOperands(expr.Operatr, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Operatr, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.Operatr, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Operatr, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !Equals(left, right);
                case TokenType.EQUAL_EQUAL:
                    return Equals(left, right);
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Handles a + b
        /// </summary>
        /// <param name="operatr"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private static object EvaluatePlus(Token operatr, object left, object right)
        {
            if (left is double || right is double)
            {
                return (double)left + (double)right;
            }

            if (left is string || right is string)
            {
                return (string)left + (string)right;
            }

            throw new RuntimeError(operatr, "Operands must be two numbers or two strings");
        }

        /// <summary>
        ///     Recursively evaluate the expression within the group (parentheses)
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        object Expr.IVisitor<object>.VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        object Expr.IVisitor<object>.VisitLiteralExpr(Literal expr)
        {
            return expr.Value;
        }

        /// <summary>
        ///     The operand of a unary expression must be evaluated first.
        ///     <para>We cast to Double before applying the operator</para>
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        object Expr.IVisitor<object>.VisitUnaryExpr(Unary expr)
        {
            var right = Evaluate(expr.Right);

            if (expr.Operatr.GetTokenType() == TokenType.MINUS)
            {
                CheckNumberOperand(expr.Operatr, right);
                return -(double)right;
            }

            if (expr.Operatr.GetTokenType() == TokenType.BANG) return !IsTruthy(right);

            return null;
        }

        internal void Interpret(Expr expr)
        {
            try
            {
                var value = Evaluate(expr);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError error)
            {
                Wagago.runtimeError(error);
            }
        }

        private static string Stringify(object value)
        {
            switch (value)
            {
                case null:
                    return "nil";
                case double:
                {
                    var text = value.ToString();
                    if (text != null && text.EndsWith(".0"))
                    {
                        text = text.Substring(0, text.Length - 2);
                    }

                    return text ?? "nil";
                }
                default:
                    return value.ToString() ?? "nil";
            }
        }

        private static void CheckNumberOperand(Token operatr, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(operatr, "Operand must be a number");
        }
        
        private static void CheckNumberOperands(Token operatr, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(operatr, "Operand must be a number");
        }

        /// <summary>
        ///  Implement Ruby's truthy rule: only false and nil are falsy
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool IsTruthy(object right)
        {
            if (right.Equals(null)) return false;
            // `is` operator checks if the run-time type of an expression result 
            // is compatible with a given type.
            if (right is bool b) return b;
            return true;
        }
    }
}