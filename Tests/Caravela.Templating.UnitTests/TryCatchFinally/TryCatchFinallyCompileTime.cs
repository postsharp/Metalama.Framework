using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class TryCatchFinallyTests
    {
        private const string TryCatchFinallyCompileTime_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        int n = 1;
        try
        {
            n = 2;
        }
        catch
        {
            n = 3;
        }
        finally
        {
            n = 4;
        }
        
        target.Parameters[0].Value = n;
        return proceed();
    }
}
";

        private const string TryCatchFinallyCompileTime_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string TryCatchFinallyCompileTime_ExpectedOutput = @"{
    a = 4;
    return a;
}";

        [Fact( Skip = "#28037 Template compiler: try/catch/finally blocks must be always marked as run-time code." )]
        public async Task TryCatchFinallyCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( TryCatchFinallyCompileTime_Template, TryCatchFinallyCompileTime_Target ) );
            testResult.AssertOutput( TryCatchFinallyCompileTime_ExpectedOutput );
        }
    }
}
