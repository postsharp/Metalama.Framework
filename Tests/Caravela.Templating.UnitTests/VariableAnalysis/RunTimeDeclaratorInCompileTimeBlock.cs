using Caravela.Framework.Impl;
using Caravela.TestFramework;
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
    [TestTemplate]
    dynamic Template()
    {
        if (target.Parameters.Count > 0)
        {
            var x = 0;
            Console.WriteLine(x);
        }
        
        if (target.Parameters.Count > 1)
        {
            var x = 1;
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
    int Method(int a, int b)
    {
        return a + b;
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
