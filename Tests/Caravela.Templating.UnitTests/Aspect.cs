// Don't rename classes, methods, neither remove namespaces. Many things are hardcoded.  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Aspects;
using Caravela.TestFramework.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;

class Aspect
{
    [Template]
    dynamic Template1()
    {
        var parameters = new object[AdviceContext.Method.Parameters.Count];
        int i = 0;
        foreach ( var p in AdviceContext.Method.Parameters )
        {
            i++;
        }

        Console.WriteLine( "Test result = " + i );

        dynamic result = AdviceContext.Proceed();
        return result;
    }
    
    [Template]
    dynamic Template2()
    {
        int i = 0;
        while ( i < AdviceContext.Method.Parameters.Count )
        {
            i++;
        }

        Console.WriteLine( "Test result = " + i );

        dynamic result = AdviceContext.Proceed();
        return result;
    }

    [Template]
    dynamic Template3()
    {
        int i = 0;

        if (AdviceContext.Method.Parameters.Count > 0)
        {
            i = 1;
        }

        Console.WriteLine( "Test result = " + i );

        dynamic result = AdviceContext.Proceed();
        return result;
    }

    [Template]
    dynamic Template4()
    {
        int i = 0;

        if ( AdviceContext.Method.Name != "some_name" )
        {
            i = 1;
        }

        Console.WriteLine( "Test result = " + i );

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

