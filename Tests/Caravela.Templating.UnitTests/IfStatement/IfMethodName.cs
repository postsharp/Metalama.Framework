using Caravela.TestFramework.Templating;
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
  [Template]
  dynamic Template()
  {
        int b;

        if (AdviceContext.Method.Name == ""Method"")
        {
            b = 1;
        }
        else
        {
            b = 0;
        }

        Console.WriteLine( b );

        dynamic result = AdviceContext.Proceed();
        return result;
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

        private const string IfMethodName_ExpectedOutput = @"
{
    Console.WriteLine(1);
    __Void result;
    return result;
}
";


        [Fact]
        public async Task IfMethodName()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfMethodName_Template, IfMethodName_Target ) );
            testResult.AssertOutput( IfMethodName_ExpectedOutput );
        }
    }
}
