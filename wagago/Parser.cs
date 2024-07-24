﻿namespace Wagago
{
    /// <summary>
    ///     Consumes a flat sequence of tokens to create the syntax tree
    ///     <para>Rules are (precedence increases downwards):</para>
    ///     <code>
    ///         program         → statement* EOF ;
    ///         statement       → exprStmt
    ///                         | printStmt ;
    ///         exprStmt        → expression ";" ;
    ///         printStmt       → "print" expression ";" ;
    ///         expression      → equality ;
    ///         equality        → comparison ( ( "!=" | "==" ) comparison )* ;
    ///         comparison      → term ( ( gt | gte | lt | lte ) term )* ;
    ///         term            → factor ( ( - | + ) factor )* ;
    ///         factor          → unary ( ( / | * ) unary )* ;
    ///         unary           → ( ! | - ) unary | primary ;
    ///         primary         → NUMBER | STRING | "true" | "false" | "nil"
    ///                         | ( expression ) ;
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
            return Equality();
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

        internal List<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd()) statements.Add(Statement());

            return statements;
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
            return Match(TokenType.PRINT) ? PrintStatement() : ExpressionStatement();
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

        private class ParserError : SystemException
        {
        }
    }
}