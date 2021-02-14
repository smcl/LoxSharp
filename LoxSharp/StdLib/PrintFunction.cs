using LoxSharp.Runtime;
using System.Linq;
using System.Collections.Generic;

namespace LoxSharp.StdLib
{
    public class PrintFunction : ILoxCallable
    {
        public int Arity => 1;

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var output = arguments.First().ToString();
            interpreter.StdOut.WriteLine(output);
            return null;
        }

        public override string ToString()
        {
            return $"<native fn print>";
        }
    }
}
