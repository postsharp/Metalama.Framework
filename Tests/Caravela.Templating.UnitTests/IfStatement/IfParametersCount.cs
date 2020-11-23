using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string ForEachParameterInput = @"
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
        bool b;

        if (AdviceContext.Method.Parameters.Count > 0)
        {
            b = true;
        }
        else
        {
            b = false;
        }

        Console.WriteLine( b );

        dynamic result = AdviceContext.Proceed();
        return result;
  }
}

class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string ForEachParameterExpectedOutput = @"
{
    bool b;
    b = true;
    Console.WriteLine(b);
    int result;
    result = a;
    return result;
}
";

        [Fact]
        public async Task IfParametersCount()
        {
            var testResult = await _testRunner.Run( ForEachParameterInput );
            testResult.AssertOutput( ForEachParameterExpectedOutput );
        }
    }
}

