using System.Collections.Generic;
using clox;
using cslox.Types;

namespace cslox
{
    public class LoxFunction: ILoxCallable
    {
        protected readonly Environment closure;
        protected readonly string name;
        protected readonly List<Stmt> body;
        protected readonly List<Token> parameters;
        protected readonly bool isInitializer;

        public LoxFunction(string name, List<Token> parameters, List<Stmt> body, Environment closure, bool isInitializer)
        {
            this.parameters = parameters;
            this.name = name;
            this.body = body;
            this.closure = closure;
            this.isInitializer = isInitializer;
            Arity = parameters?.Count ?? 0;
        }

        public int Arity { get; }

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var environment = new Environment(closure);
            if (arguments != null)
            {
                var i = 0;
                foreach (var arg in arguments)
                {
                    var parameter = parameters[i];
                    environment.Define(parameter.lexeme, arg);
                    ++i;
                }
            }
            try
            {
                interpreter.ExecuteBlock(body, environment);
            }
            catch(Return ret)
            {
                return isInitializer ? 
                    closure.GetAt(0, "this") :
                    ret.Value;
            }
            return isInitializer ? 
                closure.GetAt(0, "this") : 
                null;
        }

        public override string ToString() => $"<fn {name}>";

        public virtual LoxFunction Bind(LoxInstance instance)
        {
            var closure = new Environment(this.closure);
            closure.Define("this", instance);
            return new LoxFunction(name, parameters, body, closure, isInitializer);
        }
    }
}