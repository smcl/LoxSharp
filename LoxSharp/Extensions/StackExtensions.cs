using System.Collections.Generic;

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
            T element = default;

            var current = stack.Count;

            while (current > i)
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
    }
}
