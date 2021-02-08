using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class DynamicMetaMemberTests
    {
        private const string Generics_Template = @"  
using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.Framework.Code;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var type = target.Method.DeclaringType!.NestedTypes.GetValue().Single();
        type = type.WithGenericArguments(target.Compilation.GetTypeByReflectionType(typeof(string))!);
        var method = type.Methods.GetValue().First();
        method = method.WithGenericArguments(target.Compilation.GetTypeByReflectionType(typeof(int))!);
        _ = method.Invoke(null);
            
        return proceed();
    }
}
";

        private const string Generics_Target = @"
class TargetCode
{
    class Nested<T1> {
        static void Foo<T2>() {}
    }

    void Method()
    {
    }
}
";

        private const string Generics_ExpectedOutput = @"{
    _ = global::TargetCode.Nested<global::System.String>.Foo<global::System.Int32>();
}";

        [Fact]
        public async Task Generics()
        {
            var testResult = await this._testRunner.Run( new TestInput( Generics_Template, Generics_Target ) );
            testResult.AssertOutput( Generics_ExpectedOutput );
        }
    }
}
