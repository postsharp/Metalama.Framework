using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class LocalVariablesTests
    {
        private const string RunTimeDeclaratorInCompileTimeBlock_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        if (target.Parameters.Count > 0)
        {
            var x = 0;
            Console.WriteLine(x);
        }
        
        foreach(var p in target.Parameters)
        {
            var y = 0;
            Console.WriteLine(y);
        }
        
        return proceed();
    }
}
";

        private const string RunTimeDeclaratorInCompileTimeBlock_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string RunTimeDeclaratorInCompileTimeBlock_ExpectedOutput = @"{
    var x = 0;
    Console.WriteLine(x);
    var y = 0;
    Console.WriteLine(y);
    return a;
}";

        [Fact]
        public async Task RunTimeDeclaratorInCompileTimeBlock()
        {
            var testResult = await this._testRunner.Run( new TestInput( RunTimeDeclaratorInCompileTimeBlock_Template, RunTimeDeclaratorInCompileTimeBlock_Target ) );
            testResult.AssertOutput( RunTimeDeclaratorInCompileTimeBlock_ExpectedOutput );
        }
    }
}
