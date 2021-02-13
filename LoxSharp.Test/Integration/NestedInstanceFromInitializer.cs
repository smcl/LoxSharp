namespace LoxSharp.Test.Integration
{
    public class NestedInstanceFromInitializer : LoxTestBase
    {
        protected override string Source => @"
class X {
    init() {
        this.foo = ""bar"";
    }
}

var x = X();

print x.foo;
";
        protected override string[] ExpectedStdOutLines => new[] { "bar" };
    }
}
