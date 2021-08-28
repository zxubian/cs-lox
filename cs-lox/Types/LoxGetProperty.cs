using System.Collections.Generic;
using clox;

namespace cslox.Types
{
    public class LoxGetProperty : LoxFunction
    {
        public LoxGetProperty(string name, List<Stmt> body, Environment closure) 
            : base(name, null, body, closure, false)
        {
            
        }

        public override LoxFunction Bind(LoxInstance instance)
        {
            var closure = new Environment(this.closure);
            closure.Define("this", instance);
            return new LoxGetProperty(name, body, closure);
        }
    }
}