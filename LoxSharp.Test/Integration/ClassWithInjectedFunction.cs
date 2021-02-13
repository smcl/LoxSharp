namespace LoxSharp.Test.Integration
{
    public class ClassWithInjectedFunction : LoxTestBase
    {
        protected override string Source => @"
class X {
    init(func) { 
        this.func = func;
    }

    go() {
        this.func();
    }
}

fun foo() {
    print ""bar"";
}

var x = X(foo);

x.go();
";

        protected override string[] ExpectedStdOutLines => new string[] { "bar" };
    }
}
