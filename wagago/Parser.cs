namespace Wagago
{
    /// <summary>
    ///     Consumes a flat sequence of tokens to create the syntax tree
    ///     <para>Rules are (precedence increases downwards):</para>
    ///     <code>
    ///         program         → declaration* EOF ;
    ///         declaration     → varDecl
    ///                         | statement ;
    ///         varDecl         → "var" IDENTIFIER ( "=" expression )? ";" ;
    ///         statement       → exprStmt
    ///                         | ifStmt
    ///                         | printStmt
    ///                         | block ;
    ///         block           → "{" declaration* "}" ;
    ///         ifStmt          → "if" "(" expression ")" statement
    ///                         ( "else" statement )? ;
    ///         exprStmt        → expression ";" ;
    ///         printStmt       → "print" expression ";" ;
    ///         expression      → assignment ;
    ///         assignment      → IDENTIFIER "=" assignment
    ///                         | logic_or ;
    ///         logic_or        → logic_and ( "or" logic_and )* ;
    ///         logic_and       → equality ( "and" equality )* ;
    ///         equality        → comparison ( ( "!=" | "==" ) comparison )* ;
    ///         comparison      → term ( ( gt | gte | lt | lte ) term )* ;
    ///         term            → factor ( ( - | + ) factor )* ;
    ///         factor          → unary ( ( / | * ) unary )* ;
    ///         unary           → ( ! | - ) unary | primary ;
    ///         primary         → "true" | "false" | "nil"
    ///                         | NUMBER | STRING
    ///                         | "(" expression ")"
    ///                         | IDENTIFIER ;
    ///     </code>
    ///     <example>
    ///         For the expression "6 / 3 - 1":
    ///         <para>We first obtain the token stream:</para>
    ///         <code>
    ///             var tokens = new Scanner("6 / 3 - 1").ScanTokens()
    ///             tokens == [NUMBER, SLASH, NUMBER, MINUS, NUMBER, EOF]
    ///         </code>
    ///         <para>From the stream, we then generate the expression:</para>
    ///         <code>
    ///             var expr = new Parser(tokens).Parse()
    ///             expr.Left // Binary (6 / 3)
    ///             expr.Left.Left // Literal (6)
    ///             expr.Operatr // MINUS (-)
    ///             expr.Left.Operatr // SLASH (/)
    ///             expr.Right // Literal (1)
    ///         </code>
    ///     </example>
    /// </summary>
    public class Parser
    {
        // source token sequence
        private readonly List<Token> _tokens;
        private int _current;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Expr ParseExpression()
        {
            // descend into the equality rule
            return Assignment();
        }

        private Expr Assignment()
        {
            // 1. keep evaluating until you reach `primary` which would return an IDENTIFIER
            // cursor moves forward
            var expr = Or();

            // 2. return the identifier if there is no assignment via presence of "="
            if (!Match(TokenType.EQUAL)) return expr;
            var equals = Previous();
            // 3. evaluate the r-value (right hand side)
            var value = Assignment();

            // 4. check if the l-value is a variable i.e a 'memory' location
            if (expr is Variable variable)
            {
                var name = variable.Name;
                return new Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");

            return expr;
        }

        private Expr Or()
        {
            var expr = And();

            while (Match(TokenType.OR))
            {
                var operatr = Previous();
                var right = And();
                expr = new Logical(expr, operatr, right);
            }

            return expr;
        }

        private Expr And()
        {
            var expr = Equality();
            
            while (Match(TokenType.AND))
            {
                var operatr = Previous();
                var right = Equality();
                expr = new Logical(expr, operatr, right);
            }

            return expr;
        }

        /// <summary>
        ///     Evaluates:
        ///     <code>
        ///         equality       → comparison ( ( "!=" | "==" ) comparison )* ;
        ///     </code>
        /// </summary>
        /// <returns></returns>
        private Expr Equality()
        {
            // the first non-terminal comparison
            // descend into the comparison rule
            var expr = TokenComparison();

            // translated from ( ( "!=" | "==" ) comparison )*
            // Match uses the internal _current
            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                // the previous token; the token that was matched
                // has to be the previous token because Match advances the cursor
                var optr = Previous();
                var right = TokenComparison();
                // create a new Binary syntax tree node
                // appends the previous expressions until no more != or == is found
                expr = new Binary(expr, optr, right);
            }

            return expr;
        }

        /// <summary>
        ///     Evaluates
        ///     <code>
        ///         comparison     → term ( ( gt | gte | lt | lte ) term )* ;
        ///     </code>
        /// </summary>
        /// <returns></returns>
        private Expr TokenComparison()
        {
            var expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS,
                       TokenType.LESS_EQUAL))
            {
                var optr = Previous();
                var right = Term();
                expr = new Binary(expr, optr, right);
            }

            return expr;
        }

        /// <summary>
        ///     Evaluates:
        ///     <code>
        ///         term           → factor ( ( - | + ) factor )* ;
        ///     </code>
        /// </summary>
        /// <returns></returns>
        private Expr Term()
        {
            var expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                var optr = Previous();
                var right = Factor();
                expr = new Binary(expr, optr, right);
            }

            return expr;
        }

        /// <summary>
        ///     Evaluates:
        ///     <code>
        ///         factor         → unary ( ( / | * ) unary )* ;
        ///     </code>
        /// </summary>
        /// <returns></returns>
        private Expr Factor()
        {
            var expr = TokenUnary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                var optr = Previous();
                var right = TokenUnary();
                expr = new Binary(expr, optr, right);
            }

            return expr;
        }

        /// <summary>
        ///     Evaluates:
        ///     <code>
        ///         unary          → ( ! | - ) unary | primary ;
        ///     </code>
        /// </summary>
        /// <returns></returns>
        private Expr TokenUnary()
        {
            if (!Match(TokenType.BANG, TokenType.MINUS)) return Primary();
            var optr = Previous();
            var right = TokenUnary();
            return new Unary(optr, right);
        }

        /// <summary>
        ///     Evaluates:
        ///     <code>
        ///        primary        → NUMBER | STRING | "true" | "false" | "nil"
        ///                         | ( expression ) ;
        ///     </code>
        /// </summary>
        /// <returns></returns>
        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Literal(false);
            if (Match(TokenType.TRUE)) return new Literal(true);
            if (Match(TokenType.NIL)) return new Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
                return new Literal(Previous().GetLiteral());

            if (Match(TokenType.IDENTIFIER))
            {
                return new Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = ParseExpression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        /// <summary>
        ///     Use a token without evaluating it.
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="ParserError"></exception>
        private Token Consume(TokenType tokenType, string message)
        {
            if (Check(tokenType)) return Advance();

            throw Error(Peek(), message);
        }

        private static ParserError Error(Token token, string message)
        {
            Wagago.error(token, message);
            return new ParserError();
        }

        /// <summary>
        /// Error recovery.
        /// <para>Get the parser back to trying to parse the beginning of the next statement</para>
        /// </summary>
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().GetTokenType() == TokenType.SEMICOLON) return;

                switch (Peek().GetTokenType())
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }

        /// <summary>
        ///     Checks if the current token has any of the listed types.
        ///     <para>Advances the cursor if a match is found.</para>
        /// </summary>
        /// <param name="types">list of types to match the current token against</param>
        /// <returns>false, if the current token does not match any of the types</returns>
        private bool Match(params TokenType[] types)
        {
            if (!types.Any(Check)) return false;
            Advance();
            return true;
        }

        /// <summary>
        ///     <para>does not consume the current token</para>
        /// </summary>
        /// <param name="tokenType">type to check against</param>
        /// <returns>true, if the current token is of the same type as tokenType</returns>
        private bool Check(TokenType tokenType)
        {
            if (IsAtEnd()) return false;
            return Peek().GetTokenType() == tokenType;
        }

        /// <summary>
        ///     consumes the current token and returns it
        /// </summary>
        /// <returns></returns>
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        /// <summary>
        ///     checks if we've run out of tokens to parse
        /// </summary>
        /// <returns></returns>
        private bool IsAtEnd()
        {
            return Peek().GetTokenType() == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        /// <summary>
        ///     returns the most recently consumed token
        /// </summary>
        /// <returns></returns>
        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        /// <summary>
        /// Returns a list of declarations according to the program's grammar
        /// </summary>
        /// <returns></returns>
        internal List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd()) statements.Add(Declaration());

            return statements;
        }

        /// <summary>
        /// Parse a declaration
        /// </summary>
        /// <returns></returns>
        private Stmt Declaration()
        {
            try
            {
                return Match(TokenType.VAR) ? VarDeclaration() : Statement();
            }
            catch (ParserError error)
            {
                Synchronize();
                return null;
            }
        }

        /// <summary>
        /// Parsed the statement after the "var" keyword and handles it as either a variable
        /// definition (assignment) or declaration.
        /// <para>For example, it parses the "a = 2" in "var a = 2"</para>
        /// </summary>
        /// <returns>a Var statement node</returns>
        private Stmt VarDeclaration()
        {
            // 1. get the identifier
            var name = Consume(TokenType.IDENTIFIER, "Expect variable name");

            Expr initializer = null;
            
            // 2. if the next token is "=", we handle the variable definition
            // and parse the expression that follows
            // if not, then we handle the statement as a variable declaration
            if (Match(TokenType.EQUAL))
            {
                initializer = ParseExpression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Var(name, initializer);
        }

        /// <summary>
        ///     if the next token is a 'print' then evaluate the statement as a print statement
        ///     <para>otherwise, evaluate as an expression statement.</para>
        ///     <para></para>
        ///     <para>In both cases, the cursor advances after the match</para>
        /// </summary>
        /// <returns></returns>
        private Stmt Statement()
        {
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();

            if (Match(TokenType.LEFT_BRACE)) return new Block(ParserBlock());
            return ExpressionStatement();
        }

        private List<Stmt> ParserBlock()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        /// <summary>
        ///     the 'print' token has been consumed so we parse the expression
        /// </summary>
        /// <returns></returns>
        private Stmt PrintStatement()
        {
            var expr = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Print(expr);
        }

        private Stmt ExpressionStatement()
        {
            var expr = ParseExpression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Expression(expr);
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            var condition = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new If(condition, thenBranch, elseBranch);
        }

        private class ParserError : SystemException
        {
        }
    }
}