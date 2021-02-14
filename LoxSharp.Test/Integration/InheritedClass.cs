namespace LoxSharp.Test.Integration
{
    public class InheritedClass : LoxTestBase
    {
        protected override string Source => @"
class Doughnut {
  cook() {
    print ""Fry until golden brown."";
  }
}

class BostonCream<Doughnut { }

BostonCream().cook();
";

        protected override string[] ExpectedStdOutLines => new[] { "Fry until golden brown." };
    }
}
