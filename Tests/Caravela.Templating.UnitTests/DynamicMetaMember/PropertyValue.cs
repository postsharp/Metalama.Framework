using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class DynamicMetaMemberTests
    {
        private const string PropertyValue_Template = @"  
using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Code;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var properties = target.Method.DeclaringType!.Properties.GetValue().ToArray();

        properties[0].Value = properties[0].Value;
        _ = properties[0].GetValue(properties[0].Value);

        _ = properties[1]
            .GetIndexerValue(properties[0].Value, properties[0].Value);

        return proceed();
    }
}
";

        private const string PropertyValue_Target = @"
class TargetCode
{
    TargetCode P { get; set; }
    int this[int index] => 42;

    void Method()
    {
    }
}
";

        private const string PropertyValue_ExpectedOutput = @"{
    this.P = this.P;
    _ = this.P.P;
    _ = this.P[(global::System.Int32)(this.P)];
}";

        [Fact]
        public async Task PropertyValue()
        {
            var testResult = await this._testRunner.Run( new TestInput( PropertyValue_Template, PropertyValue_Target ) );
            testResult.AssertOutput( PropertyValue_ExpectedOutput );
        }
    }
}
