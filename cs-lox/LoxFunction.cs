using System.Collections.Generic;
using clox;
using cslox;

namespace jlox
{
    public class LoxFunction: ILoxCallable
    {
        private readonly Stmt.FunctionDecl declaration;

        public LoxFunction(Stmt.FunctionDecl declaration)
        {
            this.declaration = declaration;
            Arity = declaration.parameters.Count;
        }

        public int Arity { get; }

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var environment = new Environment(interpreter.Globals);
            var i = 0;
            foreach (var arg in arguments)
            {
                var parameter = declaration.parameters[i];
                environment.Define(parameter.lexeme, arg);
                ++i;
            }

            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch(Return ret)
            {
                return ret.Value;
            }
            return null;
        }

        public override string ToString() => $"<fn {declaration.name.lexeme}>";
    }
}