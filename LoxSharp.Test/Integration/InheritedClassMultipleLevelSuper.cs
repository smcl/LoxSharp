namespace LoxSharp.Test.Integration
{
    public class InheritedClassMultipleLevelSuper : LoxTestBase
    {
        protected override string Source => @"
class A {
  method() {
    print ""Method A"";
  }
}

class B<A {
    method()
    {
        print ""Method B"";
    }

    test()
    {
        super.method();
    }
}

class C<B { }

C().test();
";

        protected override string[] ExpectedStdOutLines => new string[] { "Method A" };
    }
}
