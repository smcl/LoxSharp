using LoxSharp.Extensions;
using System.Collections.Generic;
using Xunit;

namespace LoxSharp.Test.Unit
{
    public class StackExtensionsTests
    {
        [Fact]
        public void TestGetWorks()
        {
            // Arrange
            var stack = new Stack<int>();
            stack.Push(0);
            stack.Push(1);
            stack.Push(2);
            stack.Push(3);
            stack.Push(4);
            stack.Push(5);
            var beforeCount = stack.Count;

            // Act
            var element = stack.Get(2);
            
            // Assert
            Assert.Equal(2, element);
            Assert.Equal(beforeCount, stack.Count);
        }

        [Fact]
        public void TestIsEmptyOnEmptyStack()
        {
            // Arrange
            var stack = new Stack<int>();

            // Act
            var result = stack.IsEmpty();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void TestIsEmptyOnNonemptyStack()
        {
            // Arrange
            var stack = new Stack<int>();
            stack.Push(0);
            stack.Push(1);

            // Act
            var result = stack.IsEmpty();

            // Assert
            Assert.False(result);
        }
    }
}
