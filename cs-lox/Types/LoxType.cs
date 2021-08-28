using System.Collections.Generic;

namespace cslox.Types
{
    public class LoxType : ILoxClass
    {
        private readonly Dictionary<string, LoxFunction> staticMethods;
        public string Name => "Type";

        public LoxType(Dictionary<string, LoxFunction> staticMethods)
        {
            this.staticMethods = staticMethods;
        }

        public bool TryFindMethod(string name, out LoxFunction method) => staticMethods.TryGetValue(name, out method);

    }
}