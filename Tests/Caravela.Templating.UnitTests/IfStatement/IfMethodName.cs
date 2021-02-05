using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfMethodName_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [TestTemplate]
  dynamic Template()
  {
        int b = compileTime(0);

        if (target.Method.Name == ""Method"")
        {
            b = 1;
        }
        else
        {
            b = 2;
        }

        Console.WriteLine( b );

        return proceed();
  }
}
";

        private const string IfMethodName_Target = @"
class TargetCode
{
    void Method()
    {
    }
}
";

        private const string IfMethodName_ExpectedOutput = @"{
    Console.WriteLine(1);
}";


        [Fact]
        public async Task IfMethodName()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfMethodName_Template, IfMethodName_Target ) );
            testResult.AssertOutput( IfMethodName_ExpectedOutput );
        }
    }
}
