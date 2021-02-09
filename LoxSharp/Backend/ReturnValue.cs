using System;

namespace LoxSharp.Backend
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
