using LoxSharp.Common.Parser;
using System.Collections.Generic;

namespace LoxSharp.Backend
{
    public class Environment
    {
        private readonly Environment _enclosing;
        private readonly IDictionary<string, object> _values;

        public Environment Enclosing { get => _enclosing;  }
        public IDictionary<string, object> Values { get => _values; }

        public Environment(): this(null)
        {
        }

        public Environment(Environment enclosing)
        {
            _enclosing = enclosing;
            _values = new Dictionary<string, object>();
        }

        public void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var value))
            {
                return value;
            }

            if (_enclosing != null)
            {
                return _enclosing.Get(name);
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public object GetAt(int distance, string name)
        {
            var ancestor = Ancestor(distance);

            if (ancestor.Values.TryGetValue(name, out var value))
            {
                return value;
            }

            return null;
        }

        private Environment Ancestor(int distance)
        {
            var env = this;

            for (var i = 0; i < distance; i++)
            {
                env = env.Enclosing;
            }

            return env;
        }

        public object Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return null;
            }

            if (_enclosing != null)
            {
                _enclosing.Assign(name, value);
                return null;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void AssignAt(int distance, Token name, object value)
        {
            var ancestor = Ancestor(distance);
            ancestor.Values[name.Lexeme] = value;
        }
    }
}
