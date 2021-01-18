using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachContinueCompileTime_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        int i = compileTime(0);
        foreach (var p in target.Parameters)
        {
            if (p.Name.Length <= 1) continue;
            i++;
        }

        Console.WriteLine(i);

        dynamic result = proceed();
        return result;
    }
}
";

        private const string ForEachContinueCompileTime_Target = @"
class TargetCode
{
    int Method(int a, int bb)
    {
        return a + bb;
    }
}";

        private const string ForEachContinueCompileTime_ExpectedOutput = @"{
    Console.WriteLine(1);
    int result;
    result = a + bb;
    return (int)result;
}
";

        [Fact]
        public async Task ForEachContinueCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachContinueCompileTime_Template, ForEachContinueCompileTime_Target ) );
            testResult.AssertOutput( ForEachContinueCompileTime_ExpectedOutput );
        }
    }
}
