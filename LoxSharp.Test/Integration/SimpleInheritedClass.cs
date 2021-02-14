namespace LoxSharp.Test.Integration
{
    public class SimpleInheritedClass : LoxTestBase
    {
        protected override string Source => @"
class Doughnut {
}

class BostonCream < Doughnut {
}
";
    }
}
