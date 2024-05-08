
namespace Wagago
{
    public class Wagago
    {
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: wagago [script]");
                Environment.Exit(1);
            } else if (args.Length == 1)
            {
                runFile(args[0]);
            } else
            {
                runPrompt();
            }
        }

        private static void runFile(string filePath)
        {
            var contents = File.ReadAllText(filePath);
            run(contents);
        }

        private static void runPrompt()
        {
            try
            {
                Console.WriteLine("> ");
                while (Console.ReadLine() is { } line)
                {
                    run(line);
                    Console.WriteLine("> ");
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("{0}: The read operation could not be performed due to the " + 
                "error", e.GetType().Name);
            }
            
            
        }

        private static void run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }
    }

    public class Scanner
    {
        public Scanner(string source)
        {
            throw new NotImplementedException();
        }

        public List<Token> scanTokens()
        {
            throw new NotImplementedException();
        }
    }

    public class Token
    {
        
    }
}
