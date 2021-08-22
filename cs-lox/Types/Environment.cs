using System.Collections.Generic;
using System.Diagnostics;
using cslox;

namespace clox 
{
    public class Environment
    {
        private readonly List<string> definitions = new List<string>();
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

        public void Define(Token name)
        {
            if (Enclosing != null)
            {
                if (definitions.Contains(name.lexeme))
                {
                    throw new RuntimeError(name, $"Variable '{name.lexeme}' is already defined in this scope'.");
                }
            }
            definitions.Add(name.lexeme);
        }
        
        public void Define(string name, object value)
        {
            if (!definitions.Contains(name))
            {
                definitions.Add(name);
            }
            values[name] = value;  
        } 

        public void Define(Token name, object value)
        {
            if (Enclosing != null)
            {
                if (definitions.Contains(name.lexeme))
                {
                    throw new RuntimeError(name, $"Variable '{name.lexeme}' is already defined in this scope'.");
                }
            }
            definitions.Add(name.lexeme);
            values[name.lexeme] = value;  
        } 
        
        public object Get(Token name)
        {
            if (values.TryGetValue(name.lexeme, out var value))
            {
                return value;
            }
            if (definitions.Contains(name.lexeme))
            {
                throw new RuntimeError(name,$"Trying to access uninitialized variable '{name.lexeme}'.");
            }
            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }
            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public object GetAt(int depth, string name)
        {
            var ancestor = Ancestor(depth);
            Debug.Assert(ancestor.values.ContainsKey(name));
            return ancestor.values[name];
        }

        private Environment Ancestor(int distance)
        {
            var env = this;
            for (int i = 0; i < distance; ++i)
            {
                env = env.Enclosing;
            }
            return env;
        }

        public void Assign(Token name, object value)
        {
            var varName = name.lexeme;
            if (definitions.Contains(varName))
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
        
        public void AssignAt(int depth, Token name, object value)
        {
            var ancestor = Ancestor(depth);
            Debug.Assert(ancestor.values.ContainsKey(name.lexeme));
            ancestor.values[name.lexeme] = value;
        }
    }
}