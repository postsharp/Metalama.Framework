using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class DynamicMetaMemberTests
    {
        private const string ThisPropertyValue_Template = @"  
using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Reactive;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        foreach (var type in target.Compilation.DeclaredTypes.GetValue())
        {
            foreach (var property in type.Properties.GetValue())
                _ = property.Value;            
        }

        return proceed();
    }
}
";

        private const string ThisPropertyValue_Target = @"
class TargetCode
{
    int IP { get; set; }
    static int SP { get; set; }

    void Method()
    {
    }
}

class OtherClass
{
    public int IP { get; set; }
    public static int SP { get; set; }    
}";

        private const string ThisPropertyValue_ExpectedOutput = @"{
    _ = this.IP;
    _ = global::TargetCode.SP;
    _ = ((global::OtherClass)(this)).IP;
    _ = global::OtherClass.SP;
}";

        [Fact]
        public async Task ThisPropertyValue()
        {
            var testResult = await this._testRunner.Run( new TestInput( ThisPropertyValue_Template, ThisPropertyValue_Target ) );
            testResult.AssertOutput( ThisPropertyValue_ExpectedOutput );
        }
    }
}
