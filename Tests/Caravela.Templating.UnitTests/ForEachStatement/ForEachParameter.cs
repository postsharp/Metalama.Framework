using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachParameter_Template = @"
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
        int i = 0;
        foreach ( var p in AdviceContext.Method.Parameters )
        {
            i++;
        }

        Console.WriteLine( i );

        dynamic result = AdviceContext.Proceed();
        return result;
  }
}
";

        private const string ForEachParameter_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}";

        private const string ForEachParameter_ExpectedOutput = @"
{
    Console.WriteLine(2);
    int result;
    result = a + b;
    return result;
}
";

        [Fact]
        public async Task ForEachParameter()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachParameter_Template, ForEachParameter_Target ) );
            testResult.AssertOutput( ForEachParameter_ExpectedOutput );
        }
    }
}
