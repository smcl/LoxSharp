namespace LoxSharp.Test.Integration
{
    public class ElseStatement : LoxTestBase
    {
        protected override string Source => @"
var x = 10;
var y = 100;

if (x > y) {
    print ""x > y"";
}
else {
    print ""x <= y"";
}
";
        protected override string[] ExpectedStdOutLines => new[] { "x <= y" };
    }
}
