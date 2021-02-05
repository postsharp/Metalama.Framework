using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForStatementTests
    {
        private const string SimpleFor_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        
        
        for ( int i = 0; i < 3; i++ )
        {
            try
            {
                return proceed();
            }
            catch
            {
            }
        
        }
        
        
        throw new Exception();
    }
}
";

        private const string SimpleFor_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string SimpleFor_ExpectedOutput = @"{
    for (int i = 0; i < 3; i++)
    {
        try
        {
            return a;
        }
        catch
        {
        }
    }

    throw new Exception();
}";

        [Fact]
        public async Task SimpleFor()
        {
            var testResult = await this._testRunner.Run( new TestInput( SimpleFor_Template, SimpleFor_Target ) );
            testResult.AssertOutput( SimpleFor_ExpectedOutput );
        }
    }
}
