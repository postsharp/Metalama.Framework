using Caravela.Framework.Impl;
using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class TryCatchFinallyTests
    {
        private const string ExceptionFilterRunTime_Template = @"  
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
        catch (Exception e) when (e.GetType().Name.Contains(""DivideByZero""))
        {
            return -1;
        }
    }
}
";

        private const string ExceptionFilterRunTime_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return 42 / a;
    }
}
";

        private const string ExceptionFilterRunTime_ExpectedOutput = @"{
    try
    {
        return 42 / a;
    }
    catch (Exception e) when (e.GetType().Name.Contains(""DivideByZero""))
    {
        return (int)-1;
    }
}
";

        [Fact]
        public async Task ExceptionFilterRunTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( ExceptionFilterRunTime_Template, ExceptionFilterRunTime_Target ) );
            testResult.AssertOutput( ExceptionFilterRunTime_ExpectedOutput );
        }
    }
}
