using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachBreakCompileTime_Template = @"
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
            if (p.Name.Length > 1) break;
            i++;
        }

        Console.WriteLine(i);

        dynamic result = proceed();
        return result;
    }
}
";

        private const string ForEachBreakCompileTime_Target = @"
class TargetCode
{
    int Method(int a, int bb)
    {
        return a + bb;
    }
}";

        private const string ForEachBreakCompileTime_ExpectedOutput = @"
{
    Console.WriteLine(1);
    int result;
    result = a + bb;
    return result;
}
";

        [Fact]
        public async Task ForEachBreakCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachBreakCompileTime_Template, ForEachBreakCompileTime_Target ) );
            testResult.AssertOutput( ForEachBreakCompileTime_ExpectedOutput );
        }
    }
}
