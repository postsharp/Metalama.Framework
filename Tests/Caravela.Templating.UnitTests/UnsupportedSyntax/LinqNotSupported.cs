using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string LinqNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        dynamic result = proceed();
        
        IEnumerable<int> list = from i in new int[]{1,2,3} select i*i;
        if (result == null)
        {
            result =
                from i in list
                from i2 in list
                let ii = i * i
                where true
                orderby i, i2 descending
                join j in list on i equals j
                join j2 in list on i equals j2 into g
                group i by i2;
        }
        
        return result;
    }
}
";

        private const string LinqNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string LinqNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task LinqNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( LinqNotSupported_Template, LinqNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
