namespace LoxSharp.Test.Integration
{
    public class NumberFunction : LoxTestBase
    {
        protected override string Source => @"
fun foo(x, y) {
    return x + y;
}

print foo(10, 20);
";

        protected override string[] ExpectedStdOutLines => new[] { "30" };
    }
}
