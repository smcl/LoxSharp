namespace LoxSharp.Test.Integration
{
    public class ClassInstanceRuntimePropertyAssign : LoxTestBase
    {
        protected override string Source => @"
class X {}

var x = X();

x.foo = ""bar"";

print x.foo;
";

        protected override string[] ExpectedStdOutLines => new string[] { "bar" };
    }
}
