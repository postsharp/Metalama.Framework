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
    dynamic OverrideMethod()
    {
        string text = """";
        short c = (short) target.Parameters.Count;
        
        if (c > 0)
        {
            object s = target.Parameters[0].Name;
            if (s is string)
            {
                text = (s as string) + ""42"";
            }
        }
        
        Console.WriteLine(text);
        
        dynamic result = proceed();
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

        [Fact( Skip = "#28017 Template compiler: support for cast, as, is, typeof" )]
        public async Task CastingCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( CastingCompileTime_Template, CastingCompileTime_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
