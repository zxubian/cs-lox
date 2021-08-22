using System.Collections.Generic;
using clox;
using cslox;

namespace jlox
{
    public class LoxFunction: ILoxCallable
    {
        private readonly Environment closure;
        private readonly string name;
        private readonly List<Stmt> body;
        private readonly List<Token> parameters;

        public LoxFunction(string name, List<Token> parameters, List<Stmt> body, Environment closure)
        {
            this.parameters = parameters;
            this.name = name;
            this.body = body;
            this.closure = closure;
            Arity = parameters.Count;
        }

        public int Arity { get; }

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var environment = new Environment(closure);
            var i = 0;
            foreach (var arg in arguments)
            {
                var parameter = parameters[i];
                environment.Define(parameter.lexeme, arg);
                ++i;
            }
            try
            {
                interpreter.ExecuteBlock(body, environment);
            }
            catch(Return ret)
            {
                return ret.Value;
            }
            return null;
        }

        public override string ToString() => $"<fn {name}>";
    }
}