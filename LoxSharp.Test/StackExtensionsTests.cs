using LoxSharp.Extensions;
using System.Collections.Generic;
using Xunit;

namespace LoxSharp.Test
{
    public class StackExtensionsTests
    {
        [Fact]
        public void TestGetWorks()
        {
            // Arrange
            var underTest = new Stack<int>();
            underTest.Push(0);
            underTest.Push(1);
            underTest.Push(2);
            underTest.Push(3);
            underTest.Push(4);
            var beforeCount = underTest.Count;

            // Act
            var element = underTest.Get(2);
            
            // Assert
            Assert.Equal(2, element);
            Assert.Equal(beforeCount, underTest.Count);
        }
    }
}
