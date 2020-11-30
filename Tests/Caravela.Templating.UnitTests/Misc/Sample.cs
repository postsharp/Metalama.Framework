using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class MiscTests
    {
        private const string Sample_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [Template]
    dynamic Template()
    {
        var parameters = new object[AdviceContext.Method.Parameters.Count];
        var stringBuilder = new StringBuilder();
        AdviceContext.BuildTime( stringBuilder );
        stringBuilder.Append( AdviceContext.Method.Name );
        stringBuilder.Append( '(' );
        int i = 0;
        foreach ( var p in AdviceContext.Method.Parameters )
        {
            string comma = i > 0 ? "", "" : """";

            if ( p.IsOut )
            {
                stringBuilder.Append( $""{comma}{p.Name} = <out> "" );
            }
            else
            {
                stringBuilder.Append( $""{comma}{p.Name} = {{{i}}}"" );
                parameters[i] = p.Value;
            }

            i++;
        }
        stringBuilder.Append( ')' );

        Console.WriteLine( stringBuilder.ToString(), parameters );

        try
        {
            dynamic result = AdviceContext.Proceed();
            Console.WriteLine( stringBuilder + "" returned "" + result, parameters );
            return result;
        }
        catch ( Exception _e )
        {
            Console.WriteLine( stringBuilder + "" failed: "" + _e, parameters );
            throw;
        }
    }
}
";

        private const string Sample_Target = @"
class TargetCode
{
    int Method( int a, int b, out int c )
    {
        c = a - b;
        return a + b;
    }
}
";

        private const string Sample_ExpectedOutput = @"
{
    var parameters = new object[3];
    parameters[0] = a;
    parameters[1] = b;
    Console.WriteLine(""Method(a = {0}, b = {1}, c = <out> )"", parameters);
    try
    {
        int result;
        c = a - b;
        result = a + b;
        Console.WriteLine(""Method(a = {0}, b = {1}, c = <out> ) returned "" + result, parameters);
        return result;
    }
    catch (Exception _e)
    {
        Console.WriteLine(""Method(a = {0}, b = {1}, c = <out> ) failed: "" + _e, parameters);
        throw;
    }
}
";

        [Fact]
        public async Task Sample()
        {
            var testResult = await this._testRunner.Run( new TestInput( Sample_Template, Sample_Target ) );
            testResult.AssertOutput( Sample_ExpectedOutput );
        }
    }
}
