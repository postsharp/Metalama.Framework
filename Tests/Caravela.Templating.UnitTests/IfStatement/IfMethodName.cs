using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfMethodNameInput = @"
using System;
using System.Collections.Generic;
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
        int b;

        if (AdviceContext.Method.Name == ""Method"")
        {
            b = 1;
        }
        else
        {
            b = 0;
        }

        Console.WriteLine( b );

        dynamic result = AdviceContext.Proceed();
        return result;
  }
}

class TargetCode
{
    void Method()
    {
    }
}
";

        private const string IfMethodNameExpectedOutput = @"
{
    int b;
    b = 1;
    Console.WriteLine(b);
    int result;
    result = 1;
    return result;
}";


        [Fact]
        public async Task IfMethodName()
        {
            var testResult = await _testRunner.Run( IfMethodNameInput );
            testResult.AssertOutput( IfMethodNameExpectedOutput );
        }
    }
}
