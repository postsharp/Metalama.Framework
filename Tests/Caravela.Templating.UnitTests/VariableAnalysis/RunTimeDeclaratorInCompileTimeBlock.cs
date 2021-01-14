using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class VariableAnalysisTests
    {
        private const string RunTimeDeclaratorInCompileTimeBlock_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        var x = 0;
        
        foreach(var p in AdviceContext.Method.Parameters)
        {
            x++;
            var y = x;
            Console.WriteLine(y);
        }
        
        dynamic result = AdviceContext.Proceed();
        return result;
    }
}
";

        private const string RunTimeDeclaratorInCompileTimeBlock_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a;
    }
}
";

        private const string RunTimeDeclaratorInCompileTimeBlock_ExpectedOutput = @"";

        [Fact]
        public async Task RunTimeDeclaratorInCompileTimeBlock()
        {
            var testResult = await this._testRunner.Run( new TestInput( RunTimeDeclaratorInCompileTimeBlock_Template, RunTimeDeclaratorInCompileTimeBlock_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LocalVariableAmbiguousCoercion.Id );
        }
    }
}
