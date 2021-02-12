using System;
using System.Collections.Generic;
using System.Text;

namespace LoxSharp.Extensions
{
    public static class StackExtensions
    {
        public static bool IsEmpty<T>(this Stack<T> stack)
        {
            return stack.Count == 0;
        }

        public static T Get<T>(this Stack<T> stack, int i)
        {
            var inverseStack = new Stack<T>();
            T element = stack.Peek();


            var current = stack.Count - 1;

            while (current != i)
            {
                element = stack.Pop();
                inverseStack.Push(element);
                current -= 1;
            }

            foreach (var s in inverseStack)
            {
                stack.Push(s);
            }

            return element;
        }

        public static T GetFromTop<T>(this Stack<T> stack, int i)
        {
            var inverseStack = new Stack<T>();
            T element = stack.Peek();

            while (i > 0)
            {
                element = stack.Pop();
                inverseStack.Push(element);
                i -= 1;
            }

            foreach (var s in inverseStack)
            {
                stack.Push(s);
            }

            return element;
        }
    }
}
