using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string OperatorsCompileTime_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        int i = target.Parameters.Count;
        
        i = +-i;
        i = unchecked(i + 1);
        i = checked(i - 1);
        unchecked { i++; }
        checked { i--; }
        ++i;
        --i;
        i += 1;
        i -= 1;
        i *= 1;
        i /= 1;
        i %= 3;
        i = i * 1;
        i = i /  1;
        i = i % 3;
        i ^= 1;
        i &= 2;
        i |= 2;
        i = i ^ 1;
        i = i & 2;
        i = i | 2;
        i <<= 1;
        i >>= 1;
        i = i << 1;
        i = i >> 1;
        i = ~(~i);
        
        bool x = i switch
        {
            1 => true,
            _ => false
        };
        bool y = (i >= 2);
        
        
        var t = (x, y);
        (x, y) = t;
        
        bool? z = ((x ^ y) && y) || !x;
        
        string s = default(string);
        s ??= ""42"";
        s = s[0..2];
        
        Console.WriteLine(i);
        Console.WriteLine(t);
        Console.WriteLine(z.Value);
        Console.WriteLine(s);
        Console.WriteLine(sizeof(bool));
        Console.WriteLine(typeof(int));
        
        dynamic result = proceed();
        return result;
    }
}
";

        private const string OperatorsCompileTime_Target = @"
class TargetCode
{
    object Method(int a, int b)
    {
        return a + b;
    }
}
";

        private const string OperatorsCompileTime_ExpectedOutput = @"
{
    Console.WriteLine(2);
    Console.WriteLine((false, true));
    Console.WriteLine(true);
    Console.WriteLine(""42"");
    Console.WriteLine(sizeof(bool));
    Console.WriteLine(typeof(int));
    object result;
    result = a + b;
    return result;
}
";

        [Fact(Skip = "#28025 Template compiler: simple variable analysis. "
            + "#28114 Template compiler: C# features requiring framework > netstandard 2.0 should not be evaluated at compile time. "
            + "#28116 Template compiler: support for compile - time tuples.")]
        public async Task OperatorsCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( OperatorsCompileTime_Template, OperatorsCompileTime_Target ) );
            testResult.AssertOutput( OperatorsCompileTime_ExpectedOutput );
        }
    }
}
