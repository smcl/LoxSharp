namespace LoxSharp.Test.Integration
{
    public class PreserveScopeInBlock : LoxTestBase
    {
        protected override string Source
        {
            get => @"
var a = ""global"";
{
    fun showA()
    {
        print a;
    }

    showA();
    var a = ""block"";
    showA();
}";
        }

        protected override string[] ExpectedStdOutLines => new string[] { "global", "global" };
    }
}
