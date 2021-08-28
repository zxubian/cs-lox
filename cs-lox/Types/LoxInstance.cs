using System.Collections.Generic;

namespace cslox.Types
{
    public class LoxInstance
    {
        private LoxClass @class;
        private Dictionary<string, object> fields = new Dictionary<string, object>();
        public LoxInstance(LoxClass @class)
        {
            this.@class = @class;
        }

        public object Get(Token name)
        {
            if (fields.TryGetValue(name.lexeme, out var value))
            {
                return value;
            }
            if (@class.TryFindMethod(name.lexeme, out var method))
            {
                return method.Bind(this);
            }
            throw new RuntimeError(name, "Undefined property.");
        }

        public object Set(Token name, object value)
        {
            fields[name.lexeme] = value;
            return value;
        }

        public override string ToString() => $"{@class.Name} instance";
    }
}