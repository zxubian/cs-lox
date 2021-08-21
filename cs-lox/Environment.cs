using System.Collections.Generic;
using System.Text;
using cslox;

namespace clox 
{
    public class Environment
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        
        public readonly Environment Enclosing;

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.Enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            values[name] = value;  
        } 
        
        public object Get(Token name)
        {
            if (values.TryGetValue(name.lexeme, out var value))
            {
                return value;
            }
            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            var varName = name.lexeme;
            if (values.ContainsKey(varName))
            {
                values[varName] = value;
                return;
            }
            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }
            throw new RuntimeError(name, $"Undefined variable '{varName}'");
        }
    }
}