namespace Wagago
{
    public class Wagago
    {
        private static bool _hadError;

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
            if (_hadError) Environment.Exit(2);
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
            var expression = parser.Parse();

            if (_hadError) return;

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void error(Token token, string message)
        {
            if (token.GetTokenType() == TokenType.EOF)
            {
                Report(token.GetLine(), " at end", message);
            }
            else
            {
                Report(token.GetLine(), " at '" + token.lexeme + "'", message);
            }
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line {0}] Error {1}: {2}", line, where, message);
            _hadError = true;
        }
    }
}