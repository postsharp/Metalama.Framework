using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class VariableAnalysisTests
    {
        private const string DeclaratorInCompileTimeBlock_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        if (AdviceContext.Method.Parameters.Count > 0)
        {
            var x = 0;
            Console.WriteLine(x);
        }
        
        if (AdviceContext.Method.Parameters.Count > 1)
        {
            var x = 1;
            Console.WriteLine(x);
        }
        
        foreach(var p in AdviceContext.Method.Parameters)
        {
            var y = 0;
            Console.WriteLine(y);
        }
        
        dynamic result = AdviceContext.Proceed();
        return result;
    }
}
";

        private const string DeclaratorInCompileTimeBlock_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a;
    }
}
";

        private const string DeclaratorInCompileTimeBlock_ExpectedOutput = @"
{
    Console.WriteLine(0);
    Console.WriteLine(1);
    Console.WriteLine(0);
    Console.WriteLine(0);
    int result;
    result = a;
    return result;
}";

        [Fact]
        public async Task DeclaratorInCompileTimeBlock()
        {
            var testResult = await this._testRunner.Run( new TestInput( DeclaratorInCompileTimeBlock_Template, DeclaratorInCompileTimeBlock_Target ) );
            testResult.AssertOutput( DeclaratorInCompileTimeBlock_ExpectedOutput );
        }
    }
}
