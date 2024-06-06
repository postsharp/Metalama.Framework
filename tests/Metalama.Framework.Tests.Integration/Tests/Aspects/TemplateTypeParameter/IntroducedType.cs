using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.IntroducedType;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var type = builder.Advice.IntroduceClass( builder.Target, "IntroducedType" ).Declaration;

        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(Method),
            args: new { T = type },
            buildMethod: b => { b.Name = "FromBaseCompilation"; } );

        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(Method),
            args: new { T = type },
            buildMethod: b => { b.Name = "FromMutableCompilation"; } );
    }

    [Template]
    private void Method<[CompileTime] T>()
    {
        Console.WriteLine( typeof(T).Name );
    }
}

// <target>
[Aspect]
public class Target { }