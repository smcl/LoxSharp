namespace LoxSharp.Test.Integration
{
    public class StringAssignment : LoxTestBase
    {
        protected override string Source => @"
var x = ""hello"";
var y = ""world"";
var z = x + y;
print z;
";

        protected override string[] ExpectedStdOutLines => new[] { "helloworld" };
    }
}
