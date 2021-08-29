using System.Collections.Generic;

namespace cslox.Types
{
    public class LoxClass: LoxInstanceBase<LoxType>, ILoxCallable, ILoxClass
    {
        private readonly Dictionary<string, LoxFunction> instanceMethods;
        private readonly Dictionary<string, LoxGetProperty> instanceProperties;
        private readonly LoxClass superclass;
        public int Arity { get; }

        public LoxClass
        (
            string name, 
            LoxClass superclass,
            Dictionary<string, LoxFunction> instanceMethods, 
            Dictionary<string, LoxFunction> staticMethods, 
            Dictionary<string, LoxGetProperty> instanceProperties
        ) :base(new LoxType(staticMethods))
        {
            Name = name;
            this.instanceMethods = instanceMethods;
            this.instanceProperties = instanceProperties;
            this.superclass = superclass;
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
            instanceMethods.TryGetValue(name, out method) || 
            superclass != null &&
            superclass.TryFindMethod(name, out method);

        public bool TryFindProperty(string name, out LoxGetProperty method) =>
            instanceProperties.TryGetValue(name, out method) ||
            superclass != null &&
            superclass.TryFindProperty(name, out method);

        public string Name { get; }

        public override object Get(Token name)
        {
            if (@class.TryFindMethod(name.lexeme, out var method))
            {
                return method;
            }
            if (superclass != null && superclass.@class.TryFindMethod(name.lexeme, out method))
            {
                return method;
            }
            throw new RuntimeError(name, "Accessing undefined static member.");
        }

        public override object Set(Token name, object value)
        {
            throw new RuntimeError(name, "Static member cannot be set.");
        }

        public override string ToString() => Name;
    }
}