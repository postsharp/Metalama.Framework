using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Linq;
using Metalama.Framework.Advising;

#pragma warning disable CS0618 // IAdviceResult.AspectBuilder is obsolete

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Constructors.AdviceResult_Override;

public class TestAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.Advice.IntroduceConstructor(builder.Target, nameof(ConstructorTemplate), whenExists: OverrideStrategy.Override);

        if (result.Outcome != AdviceOutcome.Override)
        {
            throw new InvalidOperationException( $"Outcome was {result.Outcome} instead of Override." );
        }

        if (result.AdviceKind != AdviceKind.IntroduceConstructor)
        {
            throw new InvalidOperationException( $"AdviceKind was {result.AdviceKind} instead of IntroduceMethod." );
        }

        if (!builder.Advice.MutableCompilation.Comparers.Default.Equals(
                result.Declaration.ForCompilation( builder.Advice.MutableCompilation ),
                builder.Target.ForCompilation( builder.Advice.MutableCompilation ).Constructors.Single() ))
        {
            throw new InvalidOperationException( $"Declaration was not correct." );
        }
    }

    [Template]
    public void ConstructorTemplate()
    {
        Console.WriteLine( "Aspect code." );
        meta.Proceed();
    }
}

// <target>
[TestAspect]
public class TargetClass
{
    public TargetClass()
    {
        Console.WriteLine("Source code.");
    }
}