namespace LoxSharp.Test.Integration
{
    public class HelloWorldManyLines : LoxTestBase
    {
        protected override string[] ExpectedStdOutLines => new string[] { "hello", "world" };
        protected override string Source { 
            get => @"
var hello = ""hello"";
var world = ""world"";
print hello;
print world;";
        }
    }
}
