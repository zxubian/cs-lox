using System.Collections.Generic;
using cslox;

namespace jlox.Types
{
    public class LoxClass: ILoxCallable
    {
        public readonly string Name;

        public LoxClass(string name)
        {
            this.Name = name;
        }

        public override string ToString() => Name;
        public int Arity => 0;
        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            return new LoxInstance(this);
        }
    }
}