namespace Wagago
{
    /// <summary>
    /// runs before Interpreter
    /// <para>
    ///     - define scopes for blocks and functions
    /// </para>
    /// </summary>
    public class Resolver: IExpr.IVisitor<object>, IStmt.IVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new ();
        private FunctionType _currentFunction = FunctionType.NONE;
        private ClassType _currentClass = ClassType.NONE;

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        object IExpr.IVisitor<object>.VisitAssignExpr(Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Identifier);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitBinaryExpr(Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitInvocationExpr(Invocation expr)
        {
            Resolve(expr.Callee);
            foreach (var arg in expr.Arguments)
            {
                Resolve(arg);
            }

            return null!;
        }

        object IExpr.IVisitor<object>.VisitPropGetExpr(PropGet expr)
        {
            Resolve(expr.Owner);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitPropSetExpr(PropSet expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Owner);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitSuperExpr(Super expr)
        {
            if (_currentClass == ClassType.NONE)
            {
                Wagago.ReportError(expr.Keyword, "Can't use 'super' outside of a class.");
            } else if (_currentClass != ClassType.SUBCLASS)
            {
                Wagago.ReportError(expr.Keyword, "Can't use 'super' in a class with no superclass");
            }

            ResolveLocal(expr, expr.Keyword);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitGroupingExpr(Grouping expr)
        {
            Resolve(expr.Expression);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitLiteralExpr(Literal expr)
        {
            return null!;
        }

        object IExpr.IVisitor<object>.VisitLogicalExpr(Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitThisExpr(This expr)
        {
            if (_currentClass == ClassType.NONE)
            {
                Wagago.ReportError(expr.Keyword, "Can't use 'this' outside of a class");
                return null!;
            }
            ResolveLocal(expr, expr.Keyword);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitUnaryExpr(Unary expr)
        {
            Resolve(expr.Right);
            return null!;
        }

        object IExpr.IVisitor<object>.VisitVariableExpr(Variable expr)
        {

            if (_scopes.Count > 0)
            {
                var hasInScope = _scopes.Peek().TryGetValue(expr.Name.lexeme, out var scopedVariable);
                if (hasInScope && !scopedVariable)
                {
                    Wagago.ReportError(expr.Name, "Can't read local variable in its own initializer");
                }
            }

            ResolveLocal(expr, expr.Name);
            return null!;
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
        object IStmt.IVisitor<object>.VisitBlockStmt(Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null!;
        }
        
        object IStmt.IVisitor<object>.VisitExpressionStmt(Expression stmt)
        {
            Resolve(stmt.Expressn);
            return null!;
        }

        object IStmt.IVisitor<object>.VisitPrintStmt(Print stmt)
        {
            Resolve(stmt.Expression);
            return null!;
        }

        object IStmt.IVisitor<object>.VisitIfStmt(If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
            return null!;
        }

        object IStmt.IVisitor<object>.VisitWhileStmt(While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null!;
        }

        object IStmt.IVisitor<object>.VisitVarStmt(Var stmt)
        {
            Declare(stmt.Identifier);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }

            Define(stmt.Identifier);
            return null!;
        }
        
        object IStmt.IVisitor<object>.VisitReturnStmt(Return stmt)
        {
            if (_currentFunction == FunctionType.NONE)
            {
                Wagago.ReportError(stmt.Keyword, "Can't return from top-level code.");
            }
            
            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.INITIALIZER)
                {
                    Wagago.ReportError(stmt.Keyword, "Can't return a value from an initializer.");
                }
                Resolve(stmt.Value);
            }

            return null!;
        }

        object IStmt.IVisitor<object>.VisitFuncStmt(Func stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            
            ResolveFunction(stmt, FunctionType.FUNCTION);
            
            return null!;
        }

        object IStmt.IVisitor<object>.VisitImportStmt(ImportModule stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            foreach (var func in stmt.ModuleFuncs)
            {
                ResolveFunction(func, FunctionType.FUNCTION);
            }
            
            return null!;
        }
        
        /// <summary>
        /// creates a scope for the 'this' variable within the class' scope
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        object IStmt.IVisitor<object>.VisitClassStmt(Class stmt)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.CLASS;
            
            Declare(stmt.Name);
            Define(stmt.Name);

            if (stmt.SuperClass != null && stmt.Name.lexeme.Equals(stmt.SuperClass.Name.lexeme))
            {
                Wagago.ReportError(stmt.SuperClass.Name, "A class can't inherit from itself.");
            }

            if (stmt.SuperClass != null)
            {
                _currentClass = ClassType.SUBCLASS;
                Resolve(stmt.SuperClass);
            }

            // create an environment for the class as a super class
            if (stmt.SuperClass != null)
            {
                BeginScope();
                _scopes.Peek()["super"] = true;
            }
            
            BeginScope();
            _scopes.Peek()["this"] = true;

            foreach (var method in stmt.Methods)
            {
                var declaration = FunctionType.METHOD;
                if (method.Name.lexeme.Equals("init"))
                {
                    declaration = FunctionType.INITIALIZER;
                }
                ResolveFunction(method, declaration);
            }
            
            EndScope();
            
            // remove the environment for the superclass
            if (stmt.SuperClass != null) EndScope();

            _currentClass = enclosingClass;
            return null!;
        }

        internal void Resolve(List<IStmt> stmtStatements)
        {
            foreach (var stmt in stmtStatements)
            {
                Resolve(stmt);
            }
        }

        private void Resolve(IStmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(IExpr expr)
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

            if (scope.ContainsKey(stmtIdentifier.lexeme))
            {
                Wagago.ReportError(stmtIdentifier, "Already a variable with this name in this scope");
            }
            
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
        ///     scopes are stacked
        /// <para>
        ///     we start searching for the scope of the variable from the outermost scope
        /// </para>
        /// <para>
        ///     when the scope for a local variable is found, the index corresponds to the
        ///     distance from the scope to the innermost scope e.g a 0 means the scope is
        ///     the innermost scope
        /// </para>
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="exprName"></param>
        private void ResolveLocal(IExpr expr, Token exprName)
        {
            var scopesArray = _scopes.ToArray();
            for (var i = _scopes.Count - 1; i >= 0; i--)
            {
                if (!scopesArray[i].ContainsKey(exprName.lexeme)) continue;
                _interpreter.Resolve(expr, _scopes.Count - 1);
                return;
            }
        }
        
        /// <summary>
        /// define scopes for the function
        /// </summary>
        /// <param name="func"></param>
        /// <param name="functionType"></param>
        private void ResolveFunction(Func func, FunctionType functionType)
        {
            var enclosingFunction = _currentFunction;
            _currentFunction = functionType;
            BeginScope();
            foreach (var param in func.FuncParams)
            {
                Declare(param);
                Define(param);
            }
            Resolve(func.Body);
            EndScope();
            _currentFunction = enclosingFunction;
        }
    }
}