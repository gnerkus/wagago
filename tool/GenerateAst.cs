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
                    DefineAst(outputDir, "Expr",
                        new List<string>()
                        {
                            "Binary   : Expr left, Token operatr, Expr right",
                            "Grouping : Expr expression",
                            "Literal  : Object value",
                            "Unary    : Token operatr, Expr right"
                        });
                    break;
                default:
                    Console.Error.WriteLine("usage: generate_ast <output directory>");
                    Environment.Exit(1);
                    break;
            }
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            var path = $"{outputDir}/{baseName}.cs";

            using var streamWriter = new StreamWriter(path);
            streamWriter.WriteLine("namespace Wagago");
            streamWriter.WriteLine("{");
            streamWriter.WriteLine($"  abstract class {baseName}");
            streamWriter.WriteLine("  {");
            streamWriter.WriteLine("  }");
            
            foreach (var type in types)
            {
                var className = type.Split(":")[0].Trim();
                var fields = type.Split(":")[1].Trim();
                DefineType(streamWriter, baseName, className, fields);
            }
            
            // closing bracket for namespace
            streamWriter.WriteLine("}");
            streamWriter.Close();
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, 
        string fieldList)
        {
            writer.WriteLine($" class {className}: {baseName}");
            writer.WriteLine("  {");
            
            // Constructor
            writer.WriteLine($"    {className}({fieldList})");
            writer.WriteLine("    {");
            
            // -- Store parameters in fields
            var fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                writer.WriteLine($"      this.{name} = {name};");
            }
            
            writer.WriteLine("    }");
            
            // Fields
            writer.WriteLine();
            foreach (var field in fields)
            {
                writer.WriteLine($"    readonly {field};");
            }
            
            writer.WriteLine("  }");
        }
    }
}