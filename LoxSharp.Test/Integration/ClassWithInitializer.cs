namespace LoxSharp.Test.Integration
{
    public class ClassWithInitializer : LoxTestBase
    {
        protected override string Source => @"
class X { 
    init()
    {
        this.message = ""hello world"";
    }
}

var x = X();
print x.message;
";

        protected override string[] ExpectedStdOutLines => new[] { "hello world" };
    }
}
