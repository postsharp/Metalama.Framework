using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ProceedTests
    {
        private const string AssignProceedWithManyReturns_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        dynamic result = proceed();
        return result;
    }
}
";

        private const string AssignProceedWithManyReturns_Target = @"
class TargetCode
{
    bool Method(int a)
    {
        if (a % 2 == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
";

        private const string AssignProceedWithManyReturns_ExpectedOutput = @"{
    bool result;
    if (a % 2 == 0)
    {
        result = true;
        goto __continue;
    }
    else
    {
        result = false;
        goto __continue;
    }

__continue:
    ;
    return (bool)result;
}
";

        [Fact]
        public async Task AssignProceedWithManyReturns()
        {
            var testResult = await this._testRunner.Run( new TestInput( AssignProceedWithManyReturns_Template, AssignProceedWithManyReturns_Target ) );
            testResult.AssertOutput( AssignProceedWithManyReturns_ExpectedOutput );
        }
    }
}
