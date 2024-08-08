namespace Wagago
{
    /// <summary>
    ///     Custom language
    ///     我が語 (wagago); loosely translated to "my language"
    /// </summary>
    public class Wagago
    {
        private static readonly Interpreter Interpreter = new();
        private static bool _hadError;
        private static bool _hadRuntimeError;

        /**
         * Usage: wagago file.wgo
         * 
         * file.wgo contains code written in wagago
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
                    Environment.Exit(1);
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

            // TODO: create named constants for exit codes
            // exit code 2 is for source code errors
            if (_hadError) Environment.Exit(65);
            // exit code 3 is for runtime errors
            if (_hadRuntimeError) Environment.Exit(70);
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

        /**
         * Execute a line of code written in wagago
         * 
         * Sets _hadError to true if an error is encountered
         */
        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);
            var statements = parser.Parse();

            if (_hadError) return;

            var resolver = new Resolver(Interpreter);
            resolver.Resolve(statements);

            Interpreter.Interpret(statements);
            // Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void error(Token token, string message)
        {
            if (token.GetTokenType() == TokenType.EOF)
                Report(token.GetLine(), " at end", message);
            else
                Report(token.GetLine(), " at '" + token.lexeme + "'", message);
        }

        public static void runtimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.Token.GetLine()}]");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line {0}] Error {1}: {2}", line, where, message);
            _hadError = true;
        }
    }
}