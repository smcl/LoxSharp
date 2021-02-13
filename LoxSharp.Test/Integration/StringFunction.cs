namespace LoxSharp.Test.Integration
{
    public class StringFunction : LoxTestBase
    {
        protected override string Source => @"
fun greet(name) { 
    return ""hello "" + name;
}

print greet(""sean"");
";

        protected override string[] ExpectedStdOutLines => new[] { "hello sean" };
    }
}
