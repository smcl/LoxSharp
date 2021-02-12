﻿using System;
using System.Collections.Generic;

namespace LoxSharp.Backend
{
    public class LoxClass : ILoxCallable
    {
        public string Name { get; init; }
        public IDictionary<string, LoxFunction> Methods { get; init; }

        public int Arity => GetArity();

        public LoxClass(string name, IDictionary<string, LoxFunction> methods)
        {
            Name = name;
            Methods = methods;
        }

        public override string ToString()
        {
            return Name;
        }

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var instance = new LoxInstance(this);
            var initializer = FindMethod("init");

            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public LoxFunction FindMethod(string name)
        {
            if (Methods.TryGetValue(name, out var method))
            {
                return method;
            }

            return null;
        }

        private int GetArity()
        {
            var initializer = FindMethod("init");

            if (initializer != null)
            {
                return initializer.Arity;
            }

            return 0;
        }
    }
}
