using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class MiscTests
    {
        private const string SampleInput = @"  
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

class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";
        
        private const string SampleExpectedOutput = @"
{
    var parameters = new object[2];
    parameters[0] = a;
    parameters[1] = b;
    Console.WriteLine(""Method(a = {0}, b = {1})"", parameters);
    try
    {
        int result;
        result = a + b;
        Console.WriteLine(""Method(a = {0}, b = {1}) returned "" + result, parameters);
        return result;
    }
    catch (Exception _e)
    {
        Console.WriteLine(""Method(a = {0}, b = {1}) failed: "" + _e, parameters);
        throw;
    }
}
";

        [Fact]
        public async Task Sample()
        {
            var testResult = await _testRunner.Run( SampleInput );
            
            testResult.AssertOutput( SampleExpectedOutput );
        }
    }
}
