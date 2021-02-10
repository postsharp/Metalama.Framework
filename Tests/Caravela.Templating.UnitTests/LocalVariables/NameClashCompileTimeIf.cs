using System.Threading.Tasks;
using Caravela.TestFramework;
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
        object y = target.Parameters[0].Value; // run-time
        
        if (n == 1)
        {
            var x = 0;
            Console.WriteLine(x);
        }
        
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
            var x = 3;
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
    object y = a;
    var x = 0;
    Console.WriteLine(x);
    if (y == null)
    {
        var x_1 = 1;
        Console.WriteLine(x_1);
    }

    var x_2 = 2;
    Console.WriteLine(x_2);
    if (y == null)
    {
        var x_1 = 3;
        Console.WriteLine(x_1);
    }

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
