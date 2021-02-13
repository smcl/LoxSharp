using System.Collections.Generic;
using LoxSharp.Grammar;

namespace LoxSharp.Runtime
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Function _declaration;
        private readonly Environment _closure;
        private readonly bool _isInitializer;

        public int Arity => _declaration.Parameters.Count;

        public LoxFunction(Function declaration, Environment closure, bool isInitializer)
        {
            _closure = closure;
            _declaration = declaration;
            _isInitializer = isInitializer;
        }

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var env = new Environment(_closure);

            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                env.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, env);
            }
            catch (ReturnValue returnValue)
            {
                return returnValue.Value;
            }

            if (_isInitializer)
            {
                return _closure.GetAt(0, "this");
            }

            return null;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);
            return new LoxFunction(_declaration, environment, _isInitializer);
        }

        public override string ToString()
        {
            return $"<fn {_declaration.Name.Lexeme}>";
        }
    }
}
