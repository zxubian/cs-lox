using System;
using System.IO;
using System.Text;

namespace jlox
{
    
    public class jlox
    {
        private static bool hadError = false;

        public static void Main()
        {
            var expr = new Expr.Binary
            (
                new Expr.Unary
                (
                    new Token(TokenType.MINUS, "-", null, 1), 
                    new Expr.Literal(123)
                ),
                new Token(TokenType.STAR, "*", null, 1), 
                new Expr.Grouping(new Expr.Literal(45.67))
            );
            Console.WriteLine(new PrinterVisitor().Print(expr));
        }
        
        /*public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                return 64;
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
            return 0;
        }
        */

        private static void RunFile(string path)
        {
            var source = File.ReadAllText(path, Encoding.Default);
            Run(source);
            if (hadError)
            {
                Environment.Exit(65);
            }
        }

        private static void RunPrompt()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                Run(line);
                hadError = false;
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            foreach (var token in tokens)
            {
                Console.WriteLine(token.ToString());
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line.ToString()}] Error {where}: {message}");
            hadError = true;
        }

    }
}