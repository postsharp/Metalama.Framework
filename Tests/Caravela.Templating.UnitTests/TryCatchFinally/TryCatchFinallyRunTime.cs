using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class TryCatchFinallyTests
    {
        private const string TryCatchFinallyRunTime_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        try
        {
            Console.WriteLine(""try"");
            dynamic result = proceed();
            Console.WriteLine(""success"");
            return result;
        }
        catch
        {
            Console.WriteLine(""exception"");
            throw;
        }
        finally
        {
            Console.WriteLine(""finally"");
        }
    }
}
";

        private const string TryCatchFinallyRunTime_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string TryCatchFinallyRunTime_ExpectedOutput = @"{
    try
    {
        Console.WriteLine(""try"");
        int result;
        result = a;
        Console.WriteLine(""success"");
        return (int)result;
    }
    catch
    {
        Console.WriteLine(""exception"");
        throw;
    }
    finally
    {
        Console.WriteLine(""finally"");
    }
}
";

        [Fact]
        public async Task TryCatchFinallyRuntime()
        {
            var testResult = await this._testRunner.Run( new TestInput( TryCatchFinallyRunTime_Template, TryCatchFinallyRunTime_Target ) );
            testResult.AssertOutput( TryCatchFinallyRunTime_ExpectedOutput );
        }
    }
}
