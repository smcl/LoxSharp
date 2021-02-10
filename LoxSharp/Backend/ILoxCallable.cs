using System;
using System.Collections.Generic;

namespace LoxSharp.Backend
{
    public interface ILoxCallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, IList<object> arguments);
    }

    public class LoxCallable : ILoxCallable
    {
        private readonly int _arity;
        private readonly Func<IList<object>, object> _func;

        public LoxCallable(int arity, Func<IList<object>, object> func)
        {
            _arity = arity;
            _func = func;
        }

        public int Arity => _arity;

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            return _func(arguments);
        }
    }
}
