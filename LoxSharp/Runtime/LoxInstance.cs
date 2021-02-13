using LoxSharp.Common.Parser;
using System.Collections.Generic;

namespace LoxSharp.Runtime
{
    public class LoxInstance
    {
        private readonly LoxClass _klass;
        private IDictionary<string, object> _fields;
        public LoxInstance(LoxClass klass)
        {
            _klass = klass;
            _fields = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return $"{_klass.Name} instance";
        }

        public object Get(Token name)
        {
            if (_fields.TryGetValue(name.Lexeme, out var prop))
            {
                return prop;
            }

            var method = _klass.FindMethod(name.Lexeme);
            if (method != null)
            {
                return method.Bind(this);
            }

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            if (_fields.ContainsKey(name.Lexeme))
            {
                _fields[name.Lexeme] = value;
            }
            else
            {
                _fields.Add(name.Lexeme, value);
            }
        }
    }
}
