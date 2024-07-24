namespace Wagago
{
    public class Token
    {
        public readonly string lexeme;
        private readonly int line;
        private readonly object literal;
        private readonly TokenType type;

        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        public string toString()
        {
            return type + " " + lexeme + " " + literal;
        }

        public TokenType GetTokenType()
        {
            return type;
        }

        public object GetLiteral()
        {
            return literal;
        }

        public int GetLine()
        {
            return line;
        }
    }
}