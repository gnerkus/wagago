namespace Wagago
{
    /// <summary>
    ///     Custom language
    ///     我が語 (wagago); loosely translated to "my language"
    /// </summary>
    public static class Wagago
    {
        private static readonly Interpreter Interpreter = new();
        private static bool _hadError;
        private static bool _hadRuntimeError;
        
        private const int ConsoleError = 1;
        private const int CompileError = 65;
        private const int RuntimeError = 70;

        /**
         * Usage: wagago file.wgg
         * 
         * file.wgg contains code written in wagago
         * 
         * Usage: wagago
         * 
         * opens a prompt
         */
        private static void Main(string[] args)
        {
            switch (args.Length)
            {
                case > 1:
                    Console.Error.WriteLine("Usage: wagago [script]");
                    // exit code 1 is for error from console usage
                    Environment.Exit(ConsoleError);
                    break;
                case 1:
                    RunFile(args[0]);
                    break;
                default:
                    RunPrompt();
                    break;
            }
        }

        private static void RunFile(string filePath)
        {
            var contents = File.ReadAllText(filePath);
            Run(contents);

            if (_hadError) Environment.Exit(CompileError);
            if (_hadRuntimeError) Environment.Exit(RuntimeError);
        }

        private static void RunPrompt()
        {
            try
            {
                Console.WriteLine("> ");
                while (Console.ReadLine() is { } line)
                {
                    Run(line);
                    _hadError = false; // source code was executed successfully
                    Console.WriteLine("> ");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("{0}: The read operation could not be performed due to the " +
                                  "error", e.GetType().Name);
            }
        }
        
        /// <summary>
        ///     <p>Execute a line of code written in wagago</p>
        ///
        ///     <p>Sets _hadError to true if an error is encountered</p>
        /// </summary>
        /// <param name="source">code source</param>
        /// <returns></returns>
        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);
            var statements = parser.Parse();

            if (_hadError) return;

            var resolver = new Resolver(Interpreter);
            resolver.Resolve(statements);
            
            if (_hadError) return;

            Interpreter.Interpret(statements);
        }

        public static void ReportError(int line, string message)
        {
            Report(line, "", message);
        }

        public static void ReportError(Token token, string message)
        {
            if (token.GetTokenType() == TokenType.EOF)
                Report(token.GetLine(), " at end", message);
            else
                Report(token.GetLine(), " at '" + token.lexeme + "'", message);
        }

        public static void ReportRuntimeError(WagagoRuntimeException exception)
        {
            Console.Error.WriteLine($"{exception.Message}\n[line {exception.Token.GetLine()}]");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line {0}] Error {1}: {2}", line, where, message);
            _hadError = true;
        }
    }
}