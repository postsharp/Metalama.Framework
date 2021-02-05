using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private const string ReturnVoidProceedAndDefault_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        try
        {
            return proceed();
        }
        catch
        {
            return default;
        }
    }
}
";

        private const string ReturnVoidProceedAndDefault_Target = @"
using System;

class TargetCode
{
    void Method(int a, int b)
    {
        Console.WriteLine(a / b);
    }
}
";

        private const string ReturnVoidProceedAndDefault_ExpectedOutput = @"{
    try
    {
        Console.WriteLine(a / b);
    }
    catch
    {
        return;
    }
}";

        [Fact]
        public async Task ReturnVoidProceedAndDefault()
        {
            var testResult = await this._testRunner.Run( new TestInput( ReturnVoidProceedAndDefault_Template, ReturnVoidProceedAndDefault_Target ) );
            testResult.AssertOutput( ReturnVoidProceedAndDefault_ExpectedOutput );
        }
    }
}
