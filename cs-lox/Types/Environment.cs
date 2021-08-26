using System;
using System.Collections.Generic;
using cslox;

namespace clox 
{
    public class Environment
    {
        private readonly List<(bool initialized, object value)> values = new List<(bool initialized, object value)>();

        private int currentIndex = 0;
        
        public readonly Environment Enclosing;

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.Enclosing = enclosing;
        }

        public int Define(Token _)
        {
            values.Add((false, null));
            return values.Count - 1;
        }
        
        public int Define(string _, object value)
        {
            values.Add((true, value));
            return values.Count - 1;
        } 

        public int Define(Token _, object value)
        {
            values.Add((true, value));
            return values.Count - 1;
        } 
        
        public object Get(Token name, int index)
        {
            try
            {
                var (initialized, value ) = values[index];
                if (!initialized)
                {
                    throw new Exception();
                }
                return value;
            }
            catch(Exception)
            {
                throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
            }
        }

        public object GetAt(Token name, int depth, int index)
        {
            var ancestor = Ancestor(depth);
            var (initialized, value) = ancestor.values[index];
            if (initialized)
            {
                return value;
            }
            throw new RuntimeError(name, $"Undefined variable");
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

        public void Assign(Token name, int index, object value)
        {
            try
            {
                values[index] = (true, value);
            }
            catch (Exception)
            {
                throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'");
            }
        }
        
        public void AssignAt(int depth, int index, Token name, object value)
        {
            var ancestor = Ancestor(depth);
            try
            {
                ancestor.values[index] = (true, value);
            }
            catch (Exception)
            {
                throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'");
            }
        }
    }
}