namespace Wagago
{
    /// <summary>
    ///     Consumes a flat sequence of characters and break into tokens
    /// </summary>
    public class Scanner
    {
        private readonly Dictionary<string, TokenType> _keywords = new()
        {
            { "and", TokenType.AND },
            { "class", TokenType.CLASS },
            { "import", TokenType.IMPORT },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "fun", TokenType.FUN },
            { "if", TokenType.IF },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE }
        };

        // source sequence of characters
        private readonly string _source;
        private readonly char[] _sourceCharArray;
        private readonly List<Token> _tokens = new();

        private int _current;
        private int _line = 1;
        private int _start;

        public Scanner(string source)
        {
            _source = source;
            _sourceCharArray = source.ToCharArray();
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null!, _line));
            return _tokens;
        }

        /**
         * Read the character at the current cursor position (_current) the advance the cursor
         */
        private void ScanToken()
        {
            var c = Advance();

            switch (c)
            {
                case '(':
                    AddToken(TokenType.LEFT_PAREN);
                    break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN);
                    break;
                case '{':
                    AddToken(TokenType.LEFT_BRACE);
                    break;
                case '}':
                    AddToken(TokenType.RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(TokenType.COMMA);
                    break;
                case '.':
                    AddToken(TokenType.DOT);
                    break;
                case '-':
                    AddToken(TokenType.MINUS);
                    break;
                case '+':
                    AddToken(TokenType.PLUS);
                    break;
                case ';':
                    AddToken(TokenType.SEMICOLON);
                    break;
                case '*':
                    AddToken(TokenType.STAR);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                        // A comment goes until the end of the line; don't add token
                        while (Peek() != '\n' && !IsAtEnd())
                            Advance();
                    else if (Match('*'))
                        HandleBlockComment();
                    else
                        AddToken(TokenType.SLASH);
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    _line++;
                    break;
                case '"':
                    TokenizeString();
                    break;
                default:
                    if (IsDigit(c))
                        TokenizeNumber();
                    else if (IsAlpha(c))
                        Identifier();
                    else
                        Wagago.ReportError(_line, "Unexpected character.");

                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            var text = _source.Substring(_start, _current - _start);

            if (!_keywords.TryGetValue(text, out var type)) type = TokenType.IDENTIFIER;

            AddToken(type);
        }

        private static bool IsAlpha(char c)
        {
            return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private static bool IsDigit(char c)
        {
            return c is >= '0' and <= '9';
        }

        private void TokenizeNumber()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, double.Parse(_source.Substring(_start, _current - _start)));
        }

        private char PeekNext()
        {
            return _current + 1 >= _source.Length ? '\0' : _sourceCharArray[_current + 1];
        }

        private void TokenizeString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                // handle multi-line strings
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Wagago.ReportError(_line, "Unterminated string.");
                return;
            }

            // the closing ".
            Advance();

            // Trim the surrounding quotes
            var value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.STRING, value);
        }

        private void HandleBlockComment()
        {
            while (Peek() != '*' && !IsAtEnd())
            {
                // handle multi-line comment
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Wagago.ReportError(_line, "Unterminated block-style comment.");
                return;
            }

            if (Peek(1) != '/')
            {
                Wagago.ReportError(_line, "Improperly terminated block-style comment.");
                return;
            }

            Advance(2);
        }

        /**
         * Lookahead (look at the character after the one being scanned)
         */
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_sourceCharArray[_current] != expected) return false;

            _current++;
            return true;
        }

        /**
         * return the character after the one being scanned
         */
        private char Peek()
        {
            return IsAtEnd() ? '\0' : _sourceCharArray[_current];
        }

        private char Peek(int count)
        {
            return IsAtEnd() ? '\0' : _sourceCharArray[_current + count];
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        /**
         * Move the cursor forward
         */
        private char Advance()
        {
            return _sourceCharArray[_current++];
        }

        /**
         * Move the cursor forward 'count' times
         * Return the character just behind the current cursor position
         */
        private void Advance(int count)
        {
            _current += count;
        }

        private void AddToken(TokenType type, object literal = null!)
        {
            // At this point in the program,
            // _current has been incremented by the Advance method
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }
    }
}