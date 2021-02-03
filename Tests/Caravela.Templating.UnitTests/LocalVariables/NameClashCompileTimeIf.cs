using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class LocalVariablesTests
    {
        private const string NameClashCompileTimeIf_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        var n = target.Parameters.Count; // build-time
        
        if (n == 1)
        {
            var x = 0;
            Console.WriteLine(x);
        }
        
        object y = target.Parameters[0].Value;
        if (y == null)
        {
            var x = 1;
            Console.WriteLine(x);
        }
        
        if (n == 1)
        {
            var x = 2;
            Console.WriteLine(x);
        }
        
        if (y == null)
        {
            var x = 1;
            Console.WriteLine(x);
        }
        
        return proceed();
    }
}
";

        private const string NameClashCompileTimeIf_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string NameClashCompileTimeIf_ExpectedOutput = @"{
    var x = 1;
    var x1 = 2;
    return a;
}";

        [Fact]
        public async Task NameClashCompileTimeIf()
        {
            var testResult = await this._testRunner.Run( new TestInput( NameClashCompileTimeIf_Template, NameClashCompileTimeIf_Target ) );
            testResult.AssertOutput( NameClashCompileTimeIf_ExpectedOutput );
        }
    }
}
