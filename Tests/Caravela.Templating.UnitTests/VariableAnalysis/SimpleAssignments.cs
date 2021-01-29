using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class VariableAnalysisTests
    {
        private const string SimpleAssignments_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var n = target.Parameters.Count; // build-time
        //var n = runTime(target.Method.Parameters.Count); // run-time
        var a0 = target.Parameters[0].Value; // run-time
        var x = 0; // run-time
        var y = compileTime( 0 ); // compile-time    
    
        Console.WriteLine(n);
        Console.WriteLine(a0);
        Console.WriteLine(x);
        Console.WriteLine(y);
        
        return proceed();
    }
}
";

        private const string SimpleAssignments_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string SimpleAssignments_ExpectedOutput = @"{
    var a0 = a;
    var x = 0;
    Console.WriteLine(1);
    Console.WriteLine(a0);
    Console.WriteLine(x);
    Console.WriteLine(0);
    return a;
}";

        [Fact]
        public async Task SimpleAssignments()
        {
            var testResult = await this._testRunner.Run( new TestInput( SimpleAssignments_Template, SimpleAssignments_Target ) );
            testResult.AssertOutput( SimpleAssignments_ExpectedOutput );
        }
    }
}
