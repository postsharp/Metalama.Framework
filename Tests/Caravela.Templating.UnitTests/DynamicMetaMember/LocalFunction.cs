using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class DynamicMetaMemberTests
    {
        private const string LocalFunction_Template = @"  
using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Code;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var methods = target.Method.DeclaringType!.Methods.GetValue().ToList();
        
        foreach (var method in methods)
        {
            foreach (var local in method.LocalFunctions)
                _ = local.Invoke(null);
        }
            
        return proceed();
    }
}
";

        private const string LocalFunction_Target = @"
class TargetCode
{
    void Method()
    {
        void Local() {}
    }
}
";

        private const string LocalFunction_ExpectedOutput = @"{
    _ = Local();
    void Local() { }
}
";

        [Fact]
        public async Task LocalFunction()
        {
            var testResult = await this._testRunner.Run( new TestInput( LocalFunction_Template, LocalFunction_Target ) );
            testResult.AssertOutput( LocalFunction_ExpectedOutput );
        }
    }
}
