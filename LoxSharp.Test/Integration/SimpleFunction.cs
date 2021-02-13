namespace LoxSharp.Test.Integration
{
    public class SimpleFunction : LoxTestBase
    {
        protected override string Source => @"
fun greet() {
    print ""hello"";
}

greet();
";

        protected override string[] ExpectedStdOutLines => new[] { "hello" };
    }
}
