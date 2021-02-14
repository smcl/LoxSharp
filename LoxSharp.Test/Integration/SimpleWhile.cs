using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoxSharp.Test.Integration
{
    public class SimpleWhile : LoxTestBase
    {
        protected override string Source => @"

var i = 5;
while (i > 0) {
    print i;
    i = i - 1;
}
";

        protected override string[] ExpectedStdOutLines => new[] { "5", "4", "3", "2", "1" };
    }

    public class SimpleFor : LoxTestBase
    {
        protected override string Source => @"
for (var i = 5; i > 0; i = i - 1) {
    print i;
}
";

        protected override string[] ExpectedStdOutLines => new[] { "5", "4", "3", "2", "1" };
    }
}
