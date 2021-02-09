using System.Collections.Generic;

namespace LoxSharp.Backend
{
    public interface ILoxCallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, IList<object> arguments);
    }
}
