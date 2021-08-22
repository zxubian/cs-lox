using System.Collections.Generic;

namespace cslox
{
    public interface ILoxCallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, IEnumerable<object> arguments);
    }
}