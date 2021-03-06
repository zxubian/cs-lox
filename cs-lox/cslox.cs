using System;
using System.IO;
using System.Text;

namespace cslox
{
    
    public class cslox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool hadError;
        private static bool hadRuntimeError;
        
        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cs-lox [script]");
                return 64;
            }
            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
            return 0;
        }

        private static void RunFile(string path)
        {
            string source = null;
            try
            {
                source = File.ReadAllText(path, Encoding.Default);
            }
            catch (Exception e)
            {
                Error(0, $"Could not open file {path}: {e.Message}");
            }
            if (hadError)
            {
                Environment.Exit(65);
            }
            var state = new Resolver.State();
            Run(source, ref state);
            if (hadError)
            {
                Environment.Exit(65);
            }
            if (hadRuntimeError)
            {
                Environment.Exit(70);
            }
        }

        private static void RunPrompt()
        {
            var resolverState = new Resolver.State();
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                Run(line, ref resolverState);
                hadError = false;
                hadRuntimeError = false;
            }
        }

        private static void Run(string source, ref Resolver.State resolverState)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var statements = parser.Parse();
            if (hadError)
            {
                return;
            }
            var resolver = new Resolver(interpreter, ref resolverState);
            resolver.Resolve(statements);
            if (hadError)
            {
                return;
                
            }
            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }
        
        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end: ", message);
            }
            else
            {
                Report(token.line, $" at '{token.lexeme}'", message);
            }
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line.ToString()}] Error {where}: {message}");
            hadError = true;
        }

        public static void RuntimeError(RuntimeError runtimeError)
        {
            Console.WriteLine($"{runtimeError.Message}\n[line {runtimeError.Token.line}]");
            hadRuntimeError = true;
        }
    }
}