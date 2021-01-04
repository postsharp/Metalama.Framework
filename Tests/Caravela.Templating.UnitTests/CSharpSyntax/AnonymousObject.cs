using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string AnonymousObject_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        var x = new
        {
            A = AdviceContext.Method.Parameters[0].Value,
            B = AdviceContext.Method.Parameters[1].Value,
            Count = AdviceContext.Method.Parameters.Count
        };

        var y = new
        {
            Count = AdviceContext.Method.Parameters.Count
        };
        
        Console.WriteLine( x );
        Console.WriteLine( x.A );
        Console.WriteLine( x.Count );
        Console.WriteLine( y.Count );

        dynamic result = AdviceContext.Proceed();
        return result;
    }
}
";

        private const string AnonymousObject_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string AnonymousObject_ExpectedOutput = @"
{
    var x = new { A = a, B = b, Count = 2 };
    var y = new { Count = 2 };
    Console.WriteLine(x);
    Console.WriteLine(x.A);
    Console.WriteLine(x.Count);
    Console.WriteLine(y.Count);
    int result;
    result = a + b;
    return result;
}
";

        [Fact]
        public async Task AnonymousObject()
        {
            var testResult = await this._testRunner.Run( new TestInput( AnonymousObject_Template, AnonymousObject_Target ) );
            testResult.AssertOutput( AnonymousObject_ExpectedOutput );
        }
    }
}
