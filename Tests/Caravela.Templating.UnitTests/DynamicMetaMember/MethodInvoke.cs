using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class DynamicMetaMemberTests
    {
        private const string MethodInvoke_Template = @"  
using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Code;

class Aspect
{
    [Template]
    dynamic Template()
    {
        var methods = target.Method.DeclaringType!.Methods.GetValue().ToList();
        var toStringMethod = methods[0];
        var fooMethod = methods[1];
        
        _ = toStringMethod.Invoke(target.This, ""x"");
        _ = toStringMethod.Invoke(42, (long)42);
        _ = fooMethod.Invoke(null);
            
        return proceed();
    }
}
";

        private const string MethodInvoke_Target = @"
class TargetCode
{
    void ToString(string format) {}
    
    static void Foo() {}

    void Method()
    {
    }
}
";

        private const string MethodInvoke_ExpectedOutput = @"{
    _ = this.ToString(""x"");
    _ = ((global::TargetCode)(42)).ToString((global::System.String)(42L));
    _ = global::TargetCode.Foo();
}";

        [Fact]
        public async Task MethodInvoke()
        {
            var testResult = await this._testRunner.Run( new TestInput( MethodInvoke_Template, MethodInvoke_Target ) );
            testResult.AssertOutput( MethodInvoke_ExpectedOutput );
        }
    }
}
