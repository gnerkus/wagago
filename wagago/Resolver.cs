namespace Wagago
{
    public class Resolver: Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new ();

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        object Expr.IVisitor<object>.VisitAssignExpr(Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Identifier);
            return null;
        }

        object Expr.IVisitor<object>.VisitBinaryExpr(Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        object Expr.IVisitor<object>.VisitInvocationExpr(Invocation expr)
        {
            Resolve(expr.Callee);
            foreach (var arg in expr.Arguments)
            {
                Resolve(arg);
            }

            return null;
        }

        object Expr.IVisitor<object>.VisitGroupingExpr(Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        object Expr.IVisitor<object>.VisitLiteralExpr(Literal expr)
        {
            return null;
        }

        object Expr.IVisitor<object>.VisitLogicalExpr(Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        object Expr.IVisitor<object>.VisitUnaryExpr(Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        object Expr.IVisitor<object>.VisitVariableExpr(Variable expr)
        {
            if (_scopes.Count > 0 && _scopes.Peek()[expr.Name.lexeme] == false)
            {
                Wagago.error(expr.Name, "Can't read local variable in its own initializer");
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        /// <summary>
        ///
        /// <para>
        /// Every time a block is visited:
        /// 1. begin a new scope
        ///     - create a new dictionary to store the mapping for the scope
        ///     - keys are variable names
        /// 2. traverse all the statements inside the block
        ///     - map a variable name to a variable
        /// 3. discard the scope
        /// </para>
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        object Stmt.IVisitor<object>.VisitBlockStmt(Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        

        object Stmt.IVisitor<object>.VisitExpressionStmt(Expression stmt)
        {
            Resolve(stmt.Expressn);
            return null;
        }

        object Stmt.IVisitor<object>.VisitPrintStmt(Print stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        object Stmt.IVisitor<object>.VisitIfStmt(If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
            return null;
        }

        object Stmt.IVisitor<object>.VisitWhileStmt(While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        object Stmt.IVisitor<object>.VisitVarStmt(Var stmt)
        {
            Declare(stmt.Identifier);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }

            Define(stmt.Identifier);
            return null;
        }
        
        object Stmt.IVisitor<object>.VisitReturnStmt(Return stmt)
        {
            if (stmt.Value != null)
            {
                Resolve(stmt.Value);
            }

            return null;
        }

        object Stmt.IVisitor<object>.VisitFuncStmt(Func stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            
            ResolveFunction(stmt);
            return null;
        }

        internal void Resolve(List<Stmt> stmtStatements)
        {
            foreach (var stmt in stmtStatements)
            {
                Resolve(stmt);
            }
        }

        internal void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }
        
        internal void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }
        
        /// <summary>
        /// <para>
        ///     adds the variable to the innermost scope so that it shadows
        /// any outer one and so that we know the variable exists. store the variable in the scope
        /// as `false` to indicate it's not ready yet; it's initializer has not been resolved yet.
        /// </para>
        /// </summary>
        /// <param name="stmtIdentifier"></param>
        private void Declare(Token stmtIdentifier)
        {
            if (_scopes.Count <= 0) return;

            var scope = _scopes.Peek();
            scope[stmtIdentifier.lexeme] = false;
        }
        
        /// <summary>
        /// the variable is ready to be used; it's initializer has been resolved
        /// </summary>
        /// <param name="stmtIdentifier"></param>
        private void Define(Token stmtIdentifier)
        {
            if (_scopes.Count <= 0) return;

            _scopes.Peek()[stmtIdentifier.lexeme] = true;
        }
        
        /// <summary>
        /// scopes are stacked
        /// when the scope for a local variable is found, the index of the stack corresponds to
        /// the number of scopes before the variables scope was created. this is the depth
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="exprName"></param>
        private void ResolveLocal(Expr expr, Token exprName)
        {
            for (var i = _scopes.Count - 1; i >= 0; i--)
            {
                if (!_scopes.ToArray()[i].ContainsKey(exprName.lexeme)) continue;
                _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }
        
        private void ResolveFunction(Func func)
        {
            BeginScope();
            foreach (var param in func.FuncParams)
            {
                Declare(param);
                Define(param);
            }
            Resolve(func.Body);
            EndScope();
        }
    }
}