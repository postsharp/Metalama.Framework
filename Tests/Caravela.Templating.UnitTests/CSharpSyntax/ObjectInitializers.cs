using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string ObjectInitializers_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var a = new Entity1
        {
            Property1 = 1,
            Property2 =
            {
                new Entity2 { Property1 = 2 },
                new Entity2 { Property1 = 3} 
            }
        };
        
        var b = a with { Property1 = 2 };
        
        dynamic result = proceed();
        return result;
    }
}

record Entity1
{
    public int Property1 { get; set; }
    public IList<Entity2> Property2 { get; set; } = new List<Entity2>();
}

struct Entity2
{
    public int Property1 { get ;set; }
}
";

        private const string ObjectInitializers_Target = @"
class TargetCode
{
    object Method(object a)
    {
        return a;
    }
}
";

        private const string ObjectInitializers_ExpectedOutput = @"";

        [Fact]
        public async Task ObjectInitializers()
        {
            var testResult = await this._testRunner.Run( new TestInput( ObjectInitializers_Template, ObjectInitializers_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
