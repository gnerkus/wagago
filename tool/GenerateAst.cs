namespace Tool
{
    /**
     * Generates a file named "Expr.cs"
     */
    public class GenerateAst
    {
        private static void Main(string[] args)
        {
            switch (args.Length)
            {
                case > 1:
                    Console.Error.WriteLine("usage: generate_ast <output directory>");
                    Environment.Exit(1);
                    break;
                case 1:
                    var outputDir = args[0];
                    defineAst(outputDir, "Expr",
                        new List<string>()
                        {
                            "Binary   : Expr left, Token operator, Expr right",
                            "Grouping : Expr expression",
                            "Literal  : Object value",
                            "Unary    : Token operator, Expr right"
                        });
                    break;
                default:
                    Console.Error.WriteLine("usage: generate_ast <output directory>");
                    Environment.Exit(1);
                    break;
            }
        }

        private static void defineAst(string outputDir, string baseName, List<string> types)
        {
            var path = $"{outputDir}/{baseName}.cs";

            using var streamWriter = new StreamWriter(path);
            streamWriter.WriteLine("namespace Wagago");
            streamWriter.WriteLine("{");
            streamWriter.WriteLine($"abstract class {baseName}");
            streamWriter.WriteLine("{");
            streamWriter.WriteLine("}");
            streamWriter.Close();
        }
    }
}