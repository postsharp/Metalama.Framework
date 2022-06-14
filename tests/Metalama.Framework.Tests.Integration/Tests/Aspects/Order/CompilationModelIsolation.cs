using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Order.IntroductionAndOverride.CompilationModelIsolation;


[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Order.IntroductionAndOverride.CompilationModelIsolation;

internal class Aspect1 : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach ( var m in builder.Target.Methods )
        {
            builder.Advice.Override( m, nameof(this.Override) );
        }
    }

    [Introduce]
    public static void IntroducedMethod1()
    {
        Console.WriteLine( "Method introduced by Aspect1." );
    }

    [Template]
    private dynamic? Override()
    {
        Console.WriteLine(
            $"Executing Aspect1 on {meta.Target.Method.Name}. Methods present before applying Aspect1: "
            + string.Join( ", ", meta.Target.Type.Methods.Select( m => m.Name ).OrderBy( m => m ).ToArray() ) );

        return meta.Proceed();
    }
}

internal class Aspect2 : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach ( var m in builder.Target.Methods )
        {
            builder.Advice.Override( m, nameof(this.Override) );
        }
    }

    [Introduce]
    public static void IntroducedMethod2()
    {
        Console.WriteLine( "Method introduced by Aspect2." );
    }

    [Template]
    private dynamic? Override()
    {
        Console.WriteLine(
            $"Executing Aspect2 on {meta.Target.Method.Name}. Methods present before applying Aspect2: "
            + string.Join( ", ", meta.Target.Type.Methods.Select( m => m.Name ).OrderBy( m => m ).ToArray() ) );

        return meta.Proceed();
    }
}

// <target>
[Aspect1]
[Aspect2]
internal class Foo
{
    public static void SourceMethod()
    {
        Console.WriteLine( "Method defined in source code." );
    }
}