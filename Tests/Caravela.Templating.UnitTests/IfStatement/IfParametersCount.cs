using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfParametersCount_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [Template]
  dynamic Template()
  {
        bool b;

        if (AdviceContext.Method.Parameters.Count > 0)
        {
            b = true;
        }
        else
        {
            b = false;
        }

        Console.WriteLine( b );

        dynamic result = AdviceContext.Proceed();
        return result;
  }
}
";

        private const string IfParametersCount_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string IfParametersCount_ExpectedOutput = @"
{
    Console.WriteLine(true);
    int result;
    result = a;
    return result;
}";

        [Fact]
        public async Task IfParametersCount()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfParametersCount_Template, IfParametersCount_Target ) );
            testResult.AssertOutput( IfParametersCount_ExpectedOutput );
        }
    }
}

