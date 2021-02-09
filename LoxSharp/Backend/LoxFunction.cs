using System.Collections.Generic;
using LoxSharp.Grammar;

namespace LoxSharp.Backend
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Function _declaration;
        private readonly Environment _closure;

        public int Arity => _declaration.Parameters.Count;

        public LoxFunction(Function declaration, Environment closure)
        {
            _closure = closure;
            _declaration = declaration;
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

            return null;
        }

        public override string ToString()
        {
            return $"<fn {_declaration.Name.Lexeme}>";
        }
    }
}
