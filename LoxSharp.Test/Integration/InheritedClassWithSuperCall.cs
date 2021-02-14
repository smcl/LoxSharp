﻿namespace LoxSharp.Test.Integration
{
    public class InheritedClassWithSuperCall : LoxTestBase
    {
        protected override string Source => @"
class Doughnut {
  cook() {
    print ""Fry until golden brown."";
  }
}

class BostonCream<Doughnut {
    cook()
    {
        super.cook();
        print ""Pipe full of custard and coat with chocolate."";
    }
}

BostonCream().cook();
";

        protected override string[] ExpectedStdOutLines => new string[] {
            "Fry until golden brown.",
            "Pipe full of custard and coat with chocolate."
        };
    }
}
