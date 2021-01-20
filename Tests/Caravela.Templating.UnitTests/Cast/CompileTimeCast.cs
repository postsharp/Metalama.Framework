using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CastTests
    {
        private const string CompileTimeCast_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        string text = compileTime("""");
        short c = (short) target.Parameters.Count;
        
        if (c > 0)
        {
            object s = target.Parameters[0].Name;
            if (s is string)
            {
                text = (s as string) + ""42"";
            }
        }
        
        Console.WriteLine(text);
        
        return proceed();
    }
}
";

        private const string CompileTimeCast_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string CompileTimeCast_ExpectedOutput = @"{
    Console.WriteLine(""a42"");
    return a;
}";

        [Fact]
        public async Task CompileTimeCast()
        {
            var testResult = await this._testRunner.Run( new TestInput( CompileTimeCast_Template, CompileTimeCast_Target ) );
            testResult.AssertOutput( CompileTimeCast_ExpectedOutput );
        }
    }
}
