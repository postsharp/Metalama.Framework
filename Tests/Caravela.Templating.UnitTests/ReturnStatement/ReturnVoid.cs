
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private const string ReturnVoid_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        try
        {
            dynamic result = AdviceContext.Proceed();
            return result;
        }
        catch
        {
            return default;
        }
    }
}
";

        private const string ReturnVoid_Target = @"
using System;

class TargetCode
{
    void Method(int a, int b)
    {
        Console.WriteLine(a / b);
    }
}
";

        private const string ReturnVoid_ExpectedOutput = @"
{
    try
    {
        __Void result;
        Console.WriteLine(a / b);
        return result;
    }
    catch
    {
        return;
    }
}
";

        [Fact]
        public async Task ReturnVoid()
        {
            var testResult = await this._testRunner.Run( new TestInput( ReturnVoid_Template, ReturnVoid_Target ) );
            testResult.AssertOutput( ReturnVoid_ExpectedOutput );
        }
    }
}
