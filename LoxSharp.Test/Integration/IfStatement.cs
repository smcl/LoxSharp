namespace LoxSharp.Test.Integration
{
    public class IfStatement : LoxTestBase
    {
        protected override string Source => @"
var x = 100;
var y = 10;

if (x > y) {
    print ""x > y"";
}
else {
    print ""x <= y"";
}
";
        protected override string[] ExpectedStdOutLines => new[] { "x > y" };
    }
}
