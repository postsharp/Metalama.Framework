using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class VariableAnalysisTests
    {
        private const string CompileTmeDeclaratorInCompileTimeBlock_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        if (target.Parameters.Count > 0)
        {
            var x = compileTime(0);
            Console.WriteLine(x);
        }
        
        if (target.Parameters.Count > 1)
        {
            var x = compileTime(1);
            Console.WriteLine(x);
        }
        
        foreach(var p in target.Parameters)
        {
            var y = compileTime(0);
            Console.WriteLine(y);
        }
        
        return proceed();
    }
}
";

        private const string CompileTmeDeclaratorInCompileTimeBlock_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}
";

        private const string CompileTmeDeclaratorInCompileTimeBlock_ExpectedOutput = @"{
    Console.WriteLine(0);
    Console.WriteLine(1);
    Console.WriteLine(0);
    Console.WriteLine(0);
    return a + b;
}";

        [Fact]
        public async Task CompileTmeDeclaratorInCompileTimeBlock()
        {
            var testResult = await this._testRunner.Run( new TestInput( CompileTmeDeclaratorInCompileTimeBlock_Template, CompileTmeDeclaratorInCompileTimeBlock_Target ) );
            testResult.AssertOutput( CompileTmeDeclaratorInCompileTimeBlock_ExpectedOutput );
        }
    }
}
