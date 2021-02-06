using System.Threading.Tasks;
using Caravela.TestFramework;
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
  [TestTemplate]
  dynamic Template()
  {
        bool b = compileTime(false);

        if (target.Parameters.Count > 0)
        {
            b = true;
        }
        else
        {
            b = false;
        }

        Console.WriteLine( b );

        return proceed();
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

        private const string IfParametersCount_ExpectedOutput = @"{
    Console.WriteLine(true);
    return a;
}";

        [Fact]
        public async Task IfParametersCount()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfParametersCount_Template, IfParametersCount_Target ) );
            testResult.AssertOutput( IfParametersCount_ExpectedOutput );
        }
    }
}
