using System;
using System.Collections.Generic;

namespace cslox.Types
{
    public class LoxClass: ILoxCallable
    {
        private readonly Dictionary<string, LoxFunction> methods;
        
        public readonly string Name;
        public int Arity { get; }

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            this.methods = methods;
            Arity = TryFindMethod("init", out var initializer) ? 
                initializer.Arity : 0;
        }

        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            var instance = new LoxInstance(this);
            if (TryFindMethod("init", out var initializer))
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        public bool TryFindMethod(string name, out LoxFunction method) => 
            methods.TryGetValue(name, out method);
        
        public override string ToString() => Name;
    }
}