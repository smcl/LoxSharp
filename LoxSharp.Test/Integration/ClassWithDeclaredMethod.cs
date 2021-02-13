namespace LoxSharp.Test.Integration
{
    public class ClassWithDeclaredMethod : LoxTestBase
    {
        protected override string Source => @"
class X {
    greet(name) {
        print ""hello "" + name;
    }
}

X().greet(""sean"");
";
        protected override string[] ExpectedStdOutLines => new string[] { "hello sean" };
    }
}
