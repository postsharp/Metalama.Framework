using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateParameters.IntroducedType;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var type = builder.Advice.IntroduceClass( builder.Target, "IntroducedType", TypeKind.Class ).Declaration;

        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(Method),
            args: new { t = type },
            buildMethod: b => { b.Name = "FromBaseCompilation"; } );

        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(Method),
            args: new { t = type },
            buildMethod: b => { b.Name = "FromMutableCompilation"; } );
    }

    [Template]
    private void Method( [CompileTime] INamedType t )
    {
        Console.WriteLine( t.ToString() );
    }
}

// <target>
[Aspect]
public class Target { }