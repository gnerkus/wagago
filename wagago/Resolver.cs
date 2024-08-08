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
            throw new NotImplementedException();
        }

        object Expr.IVisitor<object>.VisitBinaryExpr(Binary expr)
        {
            throw new NotImplementedException();
        }

        object Expr.IVisitor<object>.VisitInvocationExpr(Invocation expr)
        {
            throw new NotImplementedException();
        }

        object Expr.IVisitor<object>.VisitGroupingExpr(Grouping expr)
        {
            throw new NotImplementedException();
        }

        object Expr.IVisitor<object>.VisitLiteralExpr(Literal expr)
        {
            throw new NotImplementedException();
        }

        object Expr.IVisitor<object>.VisitLogicalExpr(Logical expr)
        {
            throw new NotImplementedException();
        }

        object Expr.IVisitor<object>.VisitUnaryExpr(Unary expr)
        {
            throw new NotImplementedException();
        }

        object Expr.IVisitor<object>.VisitVariableExpr(Variable expr)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        object Stmt.IVisitor<object>.VisitPrintStmt(Print stmt)
        {
            throw new NotImplementedException();
        }

        object Stmt.IVisitor<object>.VisitIfStmt(If stmt)
        {
            throw new NotImplementedException();
        }

        object Stmt.IVisitor<object>.VisitWhileStmt(While stmt)
        {
            throw new NotImplementedException();
        }

        object Stmt.IVisitor<object>.VisitVarStmt(Var stmt)
        {
            throw new NotImplementedException();
        }

        object Stmt.IVisitor<object>.VisitReturnStmt(Return stmt)
        {
            throw new NotImplementedException();
        }

        object Stmt.IVisitor<object>.VisitFuncStmt(Func stmt)
        {
            throw new NotImplementedException();
        }
        
        private void Resolve(List<Stmt> stmtStatements)
        {
            foreach (var stmt in stmtStatements)
            {
                Resolve(stmt);
            }
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }
        
        private void Resolve(Expr expr)
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
    }
}