namespace Wagago
{
    /// <summary>
    ///     Consumes a flat sequence of tokens to create the syntax tree
    ///     <para>Rules are:</para>
    ///     <code>
    ///         expression     → equality ;
    ///         equality       → comparison ( ( "!=" | "==" ) comparison )* ;
    ///         comparison     → term ( ( gt | gte | lt | lte ) term )* ;
    ///         term           → factor ( ( - | + ) factor )* ;
    ///         factor         → unary ( ( / | * ) unary )* ;
    ///         unary          → ( ! | - ) unary | primary ;
    ///         primary        → NUMBER | STRING | "true" | "false" | "nil"
    ///                         | ( expression ) ;
    ///     </code>
    /// </summary>
    public class Parser
    {
        private class ParserError: SystemException {}
        
        private readonly List<Token> _tokens;
        private int _current;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Expr Expression()
        {
            // descend into the equality rule
            return Equality();
        }

        
        /// <summary>
        ///
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
        ///
        ///     Evaluates
        ///     <code>
        ///         comparison     → term ( ( gt | gte | lt | lte ) term )* ;
        ///     </code>
        /// </summary>
        /// <returns></returns>
        private Expr TokenComparison()
        {
            var expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var optr = Previous();
                var right = Term();
                expr = new Binary(expr, optr, right);
            }

            return expr;
        }

        /// <summary>
        ///
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
        ///
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
        ///
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
        ///
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
            {
                return new Literal(Previous().GetLiteral());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private Token Consume(TokenType tokenType, string message)
        {
            if (Check(tokenType)) return Advance();

            throw Error(Peek(), message);
        }

        private ParserError Error(Token token, string message)
        {
            Wagago.error(token, message);
            return new ParserError();
        }

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
        /// checks if the current token has any of the listed types
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
        ///
        ///
        /// <para>does not consume the current token</para>
        /// </summary>
        /// <param name="tokenType">type to check against</param>
        /// <returns>true, if the current token is of the same type as tokenType</returns>
        private bool Check(TokenType tokenType)
        {
            if (IsAtEnd()) return false;
            return Peek().GetTokenType() == tokenType;
        }

        /// <summary>
        /// consumes the current token and returns it
        /// </summary>
        /// <returns></returns>
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        /// <summary>
        /// checks if we've run out of tokens to parse
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
        /// returns the most recently consumed token
        /// </summary>
        /// <returns></returns>
        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        internal Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParserError e)
            {
                return null;
            }    
        }
    }
}