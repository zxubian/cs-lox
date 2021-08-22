using System;
using System.Collections.Generic;
using cslox;

namespace jlox
{
    public class Clock: ILoxCallable 
    {
        public int Arity => 0;
        public object Call(Interpreter interpreter, IEnumerable<object> arguments)
        {
            return (double)DateTime.Now.Second;
        }

        public override string ToString() => "<native fn>";
        
        public static readonly Clock Impl = new Clock();
        private Clock(){}
    }
}