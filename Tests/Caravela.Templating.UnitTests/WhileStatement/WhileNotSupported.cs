using Caravela.Framework.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class WhileStatementTests
    {
        private const string WhileNotSupportedInput = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.TestFramework.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;

class Aspect
{
    [Template]
    dynamic Template()
    {
        int i = 0;
        while ( i < AdviceContext.Method.Parameters.Count )
        {
            i++;
        }

        Console.WriteLine( ""Test result = "" + i );

        dynamic result = AdviceContext.Proceed();
        return result;
    }
}

class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        [Fact]
        public async Task WhileNotSupported()
        {
            var testResult = await _testRunner.Run( WhileNotSupportedInput );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
