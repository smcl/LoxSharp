namespace LoxSharp.Test.Integration
{
    public class SimpleArrowFunction : LoxTestBase
    {
        protected override string Source => @"
var greet = () => { print ""hello world"" };
greet();
";

        protected override string[] ExpectedStdOutLines => new[] { "hello world" };
    }
}
