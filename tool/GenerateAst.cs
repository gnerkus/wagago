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
                        new List<string>
                        {
                            "Binary   : Expr left, Token operatr, Expr right",
                            "Grouping : Expr expression",
                            "Literal  : object value",
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
            streamWriter.WriteLine($"  internal abstract class {baseName}");
            streamWriter.WriteLine("  {");
            streamWriter.WriteLine("    public abstract TR Accept<TR>(IVisitor<TR> visitor);");
            streamWriter.WriteLine("  }");

            DefineVisitor(streamWriter, baseName, types);

            // The AST classes
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

        /// <summary>
        ///     generate the visitor interface
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="baseName"></param>
        /// <param name="types"></param>
        private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine(" internal interface IVisitor<out TR> {");

            foreach (var typeName in types.Select(type => type.Split(":")[0].Trim()))
                writer.WriteLine(
                    $"    TR Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");

            writer.WriteLine("  }");
        }

        private static void DefineType(StreamWriter writer, string baseName, string className,
            string fieldList)
        {
            writer.WriteLine($" class {className}: {baseName}");
            writer.WriteLine("  {");

            // Constructor
            writer.WriteLine($"    private {className}({fieldList})");
            writer.WriteLine("    {");

            // -- Store parameters in fields
            var fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                writer.WriteLine($"      _{name} = {name};");
            }

            writer.WriteLine("    }");
            
            writer.WriteLine();
            writer.WriteLine("    public override TR Accept<TR>(IVisitor<TR> visitor)");
            writer.WriteLine("    {");
            writer.WriteLine($"      return visitor.Visit{className}{baseName}(this);");
            writer.WriteLine("    }");

            // Fields
            writer.WriteLine();
            foreach (var field in fields)
            {
                writer.WriteLine($"    private readonly {field.Replace(" ", " _")};");
            }


            writer.WriteLine("  }");
        }
    }
}