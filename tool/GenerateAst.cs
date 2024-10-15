namespace Tool
{
    /// <summary>
    /// Generates classes to represent the AST that can be created from the input formal grammar
    ///
    /// The classes are defined in the output file Expr.cs
    /// </summary>
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
                            "Assign         : Token identifier, Expr value",
                            "Binary         : Expr left, Token operatr, Expr right",
                            "Invocation     : Expr callee, Token paren, List<Expr> arguments",
                            "PropGet        : Expr owner, Token name",
                            "PropSet        : Expr owner, Token name, Expr value",
                            "Super          : Token keyword, Token method",
                            "Grouping       : Expr expression",
                            "Literal        : object value",
                            "Logical        : Expr left, Token operatr, Expr right",
                            "This           : Token keyword",
                            "Unary          : Token operatr, Expr right",
                            "Variable       : Token name"
                        });
                    DefineAst(outputDir, "Stmt",
                        new List<string>
                        {
                            "Block          : List<Stmt> statements",
                            "ImportModule   : Token name, List<Func> moduleFuncs",
                            "Class          : Token name, Variable superClass, List<Func> methods",
                            "Expression     : Expr expressn",
                            "Print          : Expr expression",
                            "If             : Expr condition, Stmt thenBranch," +
                                            " Stmt elseBranch",
                            "While          : Expr condition, Stmt body",
                            "Var            : Token identifier, Expr initializer",
                            "Return         : Token keyword, Expr value",
                            "Func           : Token name, List<Token> funcParams, List<Stmt> body" 
                        });
                    break;
                default:
                    Console.Error.WriteLine("usage: generate_ast <output directory>");
                    Environment.Exit(1);
                    break;
            }
        }

        /// <summary>
        ///     generate the abstract class for all expressions from a grammar
        /// </summary>
        /// <param name="outputDir">directory in which to place the class</param>
        /// <param name="baseName">base name for the class</param>
        /// <param name="types">the grammar in BNF</param>
        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            var path = $"{outputDir}/{baseName}.cs";

            using var streamWriter = new StreamWriter(path);
            streamWriter.WriteLine("namespace Wagago");
            streamWriter.WriteLine("{");
            streamWriter.WriteLine($"  internal abstract class {baseName}");
            streamWriter.WriteLine("  {");
            streamWriter.WriteLine("    public abstract TR Accept<TR>(IVisitor<TR> visitor);");
            streamWriter.WriteLine();

            DefineVisitor(streamWriter, baseName, types);

            streamWriter.WriteLine("  }");

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
        ///     generate the visitor interface <br />
        ///     The visitor interface contains a function definition for each expression type in the grammar.
        ///     <br />
        ///     For example
        ///     <code>
        ///         interface IVisitor {
        ///            T VisitLiteralExpr(object expr);
        ///            T VisitGroupingExpr(Expr expr);
        ///         }
        ///     </code>
        /// </summary>
        /// <param name="writer">TextWriter instance for writing to output file</param>
        /// <param name="baseName">base name for the expression class</param>
        /// <param name="types">the grammar in BNF, represented as a List</param>
        private static void DefineVisitor(TextWriter writer, string baseName, IEnumerable<string> types)
        {
            writer.WriteLine("   internal interface IVisitor<out TR> {");

            foreach (var typeName in types.Select(type => type.Split(":")[0].Trim()))
                writer.WriteLine(
                    $"      TR Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");

            writer.WriteLine("    }");
        }

        /// <summary>
        /// Defines the class for each expression type in the grammar
        /// </summary>
        /// <param name="writer">TextWriter instance</param>
        /// <param name="baseName">base name for the expression class</param>
        /// <param name="className">name of the expression type</param>
        /// <param name="fieldList">parameters for the constructor of the expression class</param>
        private static void DefineType(TextWriter writer, string baseName, string className,
            string fieldList)
        {
            writer.WriteLine($" internal class {className}: {baseName}");
            writer.WriteLine("  {");

            // Constructor
            writer.WriteLine($"    internal {className}({fieldList})");
            writer.WriteLine("    {");

            // -- Store parameters in fields
            var fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                writer.WriteLine($"      {name[0].ToString().ToUpper() + name.Substring(1)} = {name};");
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
                var splitField = field.Split(" ");
                var name = splitField[1][0].ToString().ToUpper() + splitField[1].Substring(1);
                writer.WriteLine($"    public readonly {splitField[0]} {name};");
            }


            writer.WriteLine("  }");
        }
    }
}