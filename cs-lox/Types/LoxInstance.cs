using System.Collections.Generic;

namespace cslox.Types
{
    public class LoxInstance: LoxInstanceBase<LoxClass>
    {
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
        public LoxInstance(LoxClass @class) : base(@class) {}

        public override object Get(Token nameToken)
        {
            var nameStr = nameToken.lexeme;
            if (@class.TryFindMethod(nameStr, out var method))
            {
                return method.Bind(this);
            }
            if (@class.TryFindProperty(nameStr, out var property))
            {
                return property.Bind(this);
            }
            if(fields.TryGetValue(nameStr, out var field))
            {
                return field;
            }
            throw new RuntimeError(nameToken, "Accessing undefined member.");
        }

        public override object Set(Token name, object value)
        {
            fields[name.lexeme] = value;
            return value;
        }
    }
}