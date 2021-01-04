using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
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
    [Template]
    dynamic Template()
    {
        int a = AdviceContext.Method.Parameters.Count;
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
        return AdviceContext.Proceed();
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

        [Fact]
        public async Task ExceptionFilterCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( ExceptionFilterCompileTime_Template, ExceptionFilterCompileTime_Target ) );
            testResult.AssertOutput( ExceptionFilterCompileTime_ExpectedOutput );
        }
    }
}
