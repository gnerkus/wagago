namespace Tool
{
    public class GenerateAst
    {
        private static void Main(string[] args)
        {
            switch (args.Length)
            {
                case > 1:
                    Console.Error.WriteLine("usage: generate_ast <output directory>");
                    Environment.Exit(1);
                case 1:
                    var outputDir = args[];
                default:
                    Console.Error.WriteLine("usage: generate_ast <output directory>");
                    Environment.Exit(1);
            }
        }
    }
}