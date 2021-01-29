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
    dynamic OverrideMethod()
    {
        var x = new
        {
            A = target.Parameters[0].Value,
            B = target.Parameters[1].Value,
            Count = target.Parameters.Count
        };

        var y = new
        {
            Count = target.Parameters.Count
        };
        
        Console.WriteLine( x );
        Console.WriteLine( x.A );
        Console.WriteLine( x.Count );
        Console.WriteLine( y.Count );

        dynamic result = proceed();
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

        private const string AnonymousObject_ExpectedOutput = @"{
    var x = new { A = a, B = b, Count = 2 };
    var y = new { Count = 2 };
    Console.WriteLine(x);
    Console.WriteLine(x.A);
    Console.WriteLine(x.Count);
    Console.WriteLine(y.Count);
    int result;
    result = a + b;
    return (int)result;
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
