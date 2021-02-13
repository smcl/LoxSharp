using LoxSharp.Runtime;
using System;
using System.Collections.Generic;

namespace LoxSharp.StdLib
{
    public class Clock : ILoxCallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            return (double)DateTime.UtcNow.Ticks/1000.0;
        }

        public override string ToString()
        {
            return $"<native fn {nameof(Clock).ToLower()}";
        }
    }
}
