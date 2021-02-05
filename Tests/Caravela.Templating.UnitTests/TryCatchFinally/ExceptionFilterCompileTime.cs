using Caravela.Framework.Impl;
using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class TryCatchFinallyTests
    {
        private const string ExceptionFilterCompileTime_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        int a = target.Parameters.Count;
        int b = 0;
        try
        {
            b = 100 / a;
        }
        catch (Exception e) when (e.GetType().Name.Contains(""DivideByZero""))
        {
            b = 42;
        }
        
        Console.WriteLine(b);
        return proceed();
    }
}
";

        private const string ExceptionFilterCompileTime_Target = @"
class TargetCode
{
    int Method()
    {
        return 42;
    }
}
";

        private const string ExceptionFilterCompileTime_ExpectedOutput = @"{
    a = 4;
    return a;
}";

        // NOTE: first decide whether the try/catch blocks are run-time or compile-time.
        [Fact( Skip = "#28038 Template compiler: emit a diagnostic error when template expansion throws an exception."
            + "#28037 Template compiler: try/catch/finally blocks must be always marked as run-time code." )]
        public async Task ExceptionFilterCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( ExceptionFilterCompileTime_Template, ExceptionFilterCompileTime_Target ) );
            testResult.AssertOutput( ExceptionFilterCompileTime_ExpectedOutput );
        }
    }
}
