using Caravela.Framework.Impl;
using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string UnsafeNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        int i = target.Parameters.Count;
        unsafe
        {
            int* p = &i;
            
            *p = 42;
        }

        Console.WriteLine( ""Test result = "" + i );

        dynamic result = proceed();
        return result;
    }
}
";

        private const string UnsafeNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string UnsafeNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task UnsafeNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( UnsafeNotSupported_Template, UnsafeNotSupported_Target ) );
            testResult.AssertDiagnosticId( "CS0227" );
        }
    }
}
