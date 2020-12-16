using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string CastingCompileTime_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        string text = """";
        short c = (short) AdviceContext.Method.Parameters.Count;
        
        if (c > 0)
        {
            object s = AdviceContext.Method.Parameters[0].Name;
            if (s is string)
            {
                text = (s as string) + ""42"";
            }
        }
        
        Console.WriteLine(text);
        
        dynamic result = AdviceContext.Proceed();
        return result;
    }
}
";

        private const string CastingCompileTime_Target = @"
class TargetCode
{
    string Method(int a)
    {
        return a.ToString();
    }
}
";

        private const string CastingCompileTime_ExpectedOutput = @"";

        [Fact]
        public async Task CastingCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( CastingCompileTime_Template, CastingCompileTime_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
