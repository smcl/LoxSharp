namespace LoxSharp.Test.Integration
{
    public class SimpleClass : LoxTestBase
    {
        protected override string Source => @"
class X {
}

var x = X();
";
    }
}
