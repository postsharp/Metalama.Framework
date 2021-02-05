using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForStatementTests
    {
        private const string UseForVariableInCompileTimeExpresson_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        for (int i = 0; i < target.Parameters.Count; i++)
        {
            Console.WriteLine( target.Parameters[i].Name );
        }

        dynamic result = proceed();
        return result;
    }
}
";

        private const string UseForVariableInCompileTimeExpresson_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string UseForVariableInCompileTimeExpresson_ExpectedOutput = @"";

        [Fact(Skip = "#28016 For: support for build - time loops")]
        public async Task UseForVariableInCompileTimeExpresson()
        {
            var testResult = await this._testRunner.Run( new TestInput( UseForVariableInCompileTimeExpresson_Template, UseForVariableInCompileTimeExpresson_Target ) );
            testResult.AssertOutput( UseForVariableInCompileTimeExpresson_ExpectedOutput );
        }
    }
}
