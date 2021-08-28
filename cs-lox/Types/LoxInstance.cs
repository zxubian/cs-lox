using System.Collections.Generic;
using cslox;
using cslox.Types;

namespace jlox.Types
{
    public class LoxInstance: LoxInstanceBase<LoxClass>
    {
        private readonly Dictionary<string, object> fields;
        public LoxInstance(LoxClass @class) : base(@class) { 
        }

        public override object Get(Token nameToken)
        {
            var nameStr = nameToken.lexeme;
            if (@class.TryFindMethod(nameStr, out var method))
            {
                return method;
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