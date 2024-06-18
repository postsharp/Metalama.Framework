using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.DynamicInLambda2;

#pragma warning disable CS0618 // Type or member is obsolete

internal class Aspect
{
    [TestTemplate]
    private dynamic? Template()
    {
        Console.WriteLine( string.Join( ", ", meta.Target.Parameters.Select( p => p.Value?.ToString() ) ) );

        return meta.Proceed();
    }
}

internal class TargetCode
{
    private int Method( int a, int b )
    {
        return a + b;
    }
}