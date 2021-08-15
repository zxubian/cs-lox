using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace cslox
{
    public class GenerateAst
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generate_ast <output directory>");
                return 64;
            }
            var outputDir = args[0];
            DefineAst(outputDir, "Expr", new List<string>
            {
                "Binary: Expr left, Token operatorToken, Expr right",
                "Grouping: Expr expression",
                "Literal: Object value",
                "Unary: Token operatorToken, Expr right",
                "Ternary: Expr left, Token firstOperator, Expr mid, Token secondOperator, Expr right"
            });
            return 0;
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            var path = $"{Path.Combine(outputDir, baseName)}.cs";
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine("namespace cslox");
                writer.WriteLine("{");
                writer.WriteLine();
                writer.WriteLine($"\tpublic abstract class {baseName}");
                writer.WriteLine("\t{");
                DefineVisitor(writer, baseName, types);
                foreach (var type in types)
                {
                    var split = type.Split(":");
                    var className = split[0].Trim();
                    var fields = split[1].Trim();
                    DefineType(writer, baseName, className, fields);
                }
                writer.WriteLine();
                writer.WriteLine("\t\tpublic abstract T Accept<T>(IVisitor<T> visitor);");
                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }

        private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine("\t\tpublic interface IVisitor<T>");
            writer.WriteLine("\t\t{");
            foreach (var typeName in types.Select(type => type.Split(":")[0].Trim()))
            {
                writer.WriteLine($"\t\t\t T Visit{typeName}{baseName}({typeName} {baseName.ToLowerInvariant()});");
            }
            writer.WriteLine("\t\t}");
        }

        private static void DefineType(StreamWriter writer, string baseName, string classname, string fieldList)
        {
            // class
            writer.WriteLine($"\t\tpublic class {classname} : {baseName}");
            writer.WriteLine("\t\t{");
            // constructor
            writer.WriteLine($"\t\t\tpublic {classname} ({fieldList})");
            writer.WriteLine("\t\t\t{");
            var fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                writer.WriteLine($"\t\t\tthis.{name} = {name};");
            }
            writer.WriteLine("\t\t\t}");
            writer.WriteLine();
            // Fields
            foreach (var field in fields)
            {
                writer.WriteLine($"\t\t\tpublic readonly {field};");
            }
            // Visitor
            writer.WriteLine($"\t\t\tpublic override T Accept<T>(IVisitor<T> visitor) => visitor.Visit{classname}{baseName}(this);");
            writer.WriteLine("\t\t}");
        }
    }
}
