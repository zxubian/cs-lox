using System.Collections.Generic;
using cslox;

namespace jlox.Types
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