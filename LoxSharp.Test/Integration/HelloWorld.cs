namespace LoxSharp.Test.Integration
{
    public class HelloWorld : LoxTestBase
    {
        protected override string[] ExpectedStdOutLines => new string[] { "hello world" };
        protected override string Source
        {
            get => @"
print ""hello world"";";
        }
    }
}
