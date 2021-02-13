using System;

namespace LoxSharp.Runtime
{
    public class ReturnValue : Exception
    {
        public object Value { get; }

        public ReturnValue(object value)
        {
            this.Value = value;
        }
    }
}
