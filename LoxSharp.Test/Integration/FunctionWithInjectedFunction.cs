namespace LoxSharp.Test.Integration
{
    public class FunctionWithInjectedFunction : LoxTestBase
    {
        protected override string Source => @"
fun foo(f) { 
    f();
}

fun bar() {
    print ""bar"";
}

foo(bar);
";

        protected override string[] ExpectedStdOutLines => new string[] { "bar" };
    }
}
