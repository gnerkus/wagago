﻿namespace Wagago
{
    /// <summary>
    ///
    /// <para>native functions are stored in the global scope (environment)</para>
    /// </summary>
    public class Interpreter : IExpr.IVisitor<object>, Stmt.IVisitor<object>
    {
        internal readonly Env Globals = new();
        private Env _environment;
        private readonly Dictionary<IExpr, int> _locals = new();

        public Interpreter()
        {
            _environment = Globals;
            Globals.Define("clock", new WagagoClock());
        }

        internal void Resolve(IExpr expr, int depth)
        {
            _locals[expr] = depth;
        }
        
        object IExpr.IVisitor<object>.VisitBinaryExpr(Binary expr)
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
            }

            return null!;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// <exception cref="RuntimeError">Thrown when the expressions callee is not a callable</exception>
        /// <exception cref="RuntimeError">Thrown when there are too few or too many function arguments</exception>
        object IExpr.IVisitor<object>.VisitInvocationExpr(Invocation expr)
        {
            var callee = Evaluate(expr.Callee);

            var args = expr.Arguments.Select(Evaluate).ToList();

            if (callee is not IWagagoCallable callable)
            {
                throw new RuntimeError(expr.Paren, "Can only call functions and classes");
            }

            if (args.Count != callable.Arity())
            {
                throw new RuntimeError(expr.Paren,
                    $"Expected {callable.Arity()} arguments but got {args.Count}.");
            }

            return callable.Invocation(this, args);
        }

        object IExpr.IVisitor<object>.VisitPropGetExpr(PropGet expr)
        {
            var owner = Evaluate(expr.Owner);
            if (owner is WagagoInstance instance)
            {
                return instance.Get(expr.Name);
            }

            throw new RuntimeError(expr.Name, "Only instances have properties");
        }

        object IExpr.IVisitor<object>.VisitPropSetExpr(PropSet expr)
        {
            var owner = Evaluate(expr.Owner);

            if (owner is not WagagoInstance instance)
            {
                throw new RuntimeError(expr.Name, "Only instances have fields");
            }

            var value = Evaluate(expr.Value);
            instance.Set(expr.Name, value);
            return value;
        }

        /// <summary>
        /// Evaluates an expression containing the keyword 'super' and returns the associated method
        /// <para>
        ///     The method is bound to the `this` context of the parent of the class where the `super`
        ///     expression was declared.
        /// </para>
        /// <para>
        ///     'super' only applies to methods and not fields. 
        /// </para>
        /// <para>
        ///     1. 
        /// </para>
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        object IExpr.IVisitor<object>.VisitSuperExpr(Super expr)
        {
            // the distance of the scope in which `super` was declared from the current scope
            var distance = _locals[expr];
            var superClass = (WagagoClass)_environment.GetAt(distance, "super");
            // the superclass to which `super` refers
            var obj = (WagagoInstance)_environment.GetAt(distance - 1, "this");
            var method = superClass.FindMethod(expr.Method.lexeme);

            if (method == null)
            {
                throw new RuntimeError(expr.Method, $"Undefined property '{expr.Method.lexeme}'.");
            }
            
            return method.Bind(obj);
        }

        /// <summary>
        ///     Recursively evaluate the expression within the group (parentheses)
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        object IExpr.IVisitor<object>.VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        object IExpr.IVisitor<object>.VisitLiteralExpr(Literal expr)
        {
            return expr.Value;
        }

        object IExpr.IVisitor<object>.VisitLogicalExpr(Logical expr)
        {
            var left = Evaluate(expr.Left);

            if (expr.Operatr.GetTokenType() == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        object IExpr.IVisitor<object>.VisitThisExpr(This expr)
        {
            return LookUpVariable(expr.Keyword, expr);
        }

        /// <summary>
        ///     The operand of a unary expression must be evaluated first.
        ///     <para>We cast to Double before applying the operator</para>
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        object IExpr.IVisitor<object>.VisitUnaryExpr(Unary expr)
        {
            var right = Evaluate(expr.Right);

            if (expr.Operatr.GetTokenType() == TokenType.MINUS)
            {
                CheckNumberOperand(expr.Operatr, right);
                return -(double)right;
            }

            if (expr.Operatr.GetTokenType() == TokenType.BANG) return !IsTruthy(right);

            return null!;
        }

        object IExpr.IVisitor<object>.VisitVariableExpr(Variable expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        object Stmt.IVisitor<object>.VisitBlockStmt(Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Env(_environment));
            return null!;
        }

        /// <summary>
        ///     Execute class definition; define a class
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        /// <exception cref="RuntimeError"></exception>
        object Stmt.IVisitor<object>.VisitClassStmt(Class stmt)
        {
            object superClass = null!;
            _environment.Define(stmt.Name.lexeme, null!);

            if (stmt.SuperClass != null)
            {
                superClass = Evaluate(stmt.SuperClass);
                if (superClass is not WagagoClass)
                {
                    throw new RuntimeError(stmt.SuperClass.Name, "Superclass must be a class");
                }
                
                // add a ref to the super class to the environment
                _environment = new Env(_environment);
                _environment.Define("super", superClass);
            }

            var methods = new Dictionary<string, WagagoFunction>();
            foreach (var method in stmt.Methods)
            {
                var func = new WagagoFunction(method, _environment, method.Name.lexeme.Equals("init"));
                methods.Add(method.Name.lexeme, func);
            }

            var klass = new WagagoClass(stmt.Name.lexeme, (WagagoClass)superClass, methods);

            if (superClass != null)
            {
                _environment = _environment.Enclosing!;
            }

            _environment?.Assign(stmt.Name, klass);

            return null!;
        }

        object Stmt.IVisitor<object>.VisitExpressionStmt(Expression stmt)
        {
            Evaluate(stmt.Expressn);
            return null!;
        }

        object Stmt.IVisitor<object>.VisitPrintStmt(Print stmt)
        {
            var value = Evaluate(stmt.Expression);
            Console.WriteLine(Stringify(value));
            return null!;
        }

        object Stmt.IVisitor<object>.VisitIfStmt(If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return null!;
        }

        object Stmt.IVisitor<object>.VisitWhileStmt(While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return null!;
        }

        /// <summary>
        /// Handle the Var statement node.
        /// <para>Store the variable's value as null in the environment
        /// if there is no initializer</para>
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        object Stmt.IVisitor<object>.VisitVarStmt(Var stmt)
        {
            object value = null!;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _environment.Define(stmt.Identifier.lexeme, value);
            return null!;
        }

        object Stmt.IVisitor<object>.VisitReturnStmt(Return stmt)
        {
            object value = null!;
            if (stmt.Value != null) value = Evaluate(stmt.Value);

            throw new ReturnException(value);
        }

        object Stmt.IVisitor<object>.VisitFuncStmt(Func stmt)
        {
            var func = new WagagoFunction(stmt, _environment, false);
            _environment.Define(stmt.Name.lexeme, func);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitAssignExpr(Assign expr)
        {
            var value = Evaluate(expr.Value);

            var hasDistance = _locals.TryGetValue(expr, out var distance);
            if (hasDistance)
            {
                _environment.AssignAt(distance, expr.Identifier, value);
            }
            else
            {
                Globals.Assign(expr.Identifier, value);
            }

            return value;
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
            if (left is double || right is double) return (double)left + (double)right;

            if (left is string || right is string) return (string)left + (string)right;

            throw new RuntimeError(operatr, "Operands must be two numbers or two strings");
        }

        private object Evaluate(IExpr expr)
        {
            return expr.Accept(this);
        }

        internal void ExecuteBlock(List<Stmt> statements, Env blockEnv)
        {
            var previous = _environment;

            try
            {
                _environment = blockEnv;

                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        /// <summary>
        /// similar to the Evaluate method for handling expressions
        /// <para>no value is returned as we're not evaluating an expression</para>
        /// </summary>
        /// <param name="stmt"></param>
        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        internal void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (var stmt in statements) Execute(stmt);
            }
            catch (RuntimeError error)
            {
                Wagago.ReportRuntimeError(error);
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
                        text = text.Substring(0, text.Length - 2);

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
        ///     Implement Ruby's truthy rule: only false and null are falsy
        /// </summary>
        /// <param name="right"></param>
        /// <returns></returns>
        private static bool IsTruthy(object right)
        {
            if (right == null) return false;
            // `is` operator checks if the run-time type of an expression result 
            // is compatible with a given type.
            if (right is bool b) return b;
            return true;
        }
        
        private object LookUpVariable(Token exprName, IExpr expr)
        {
            var hasDistance = _locals.TryGetValue(expr, out var distance);
            return hasDistance ? _environment.GetAt(distance, exprName.lexeme) : Globals.Get(exprName);
        }
    }
}