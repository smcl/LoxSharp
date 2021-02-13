namespace LoxSharp.Test.Integration
{
    public class NumberAssignment : LoxTestBase
    {
        protected override string Source => @"
var x = 10;
var y = 20;
var z = x + y;
print z;
";

        protected override string[] ExpectedStdOutLines => new[] { "30" };
    }
}
