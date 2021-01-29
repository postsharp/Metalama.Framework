using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private const string ReturnVoidResultAndNull_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        try
        {
            dynamic result = proceed();
            return result;
        }
        catch
        {
            return null;
        }
    }
}
";

        private const string ReturnVoidResultAndNull_Target = @"
using System;

class TargetCode
{
    void Method(int a, int b)
    {
        Console.WriteLine(a / b);
    }
}
";

        private const string ReturnVoidResultAndNull_ExpectedOutput = @"{
    try
    {
        __Void result;
        Console.WriteLine(a / b);
        return;
    }
    catch
    {
        return;
    }
}";

        [Fact]
        public async Task ReturnVoidResultAndNull()
        {
            var testResult = await this._testRunner.Run( new TestInput( ReturnVoidResultAndNull_Template, ReturnVoidResultAndNull_Target ) );
            testResult.AssertOutput( ReturnVoidResultAndNull_ExpectedOutput );
        }
    }
}
