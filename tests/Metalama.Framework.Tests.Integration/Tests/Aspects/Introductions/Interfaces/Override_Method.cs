using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.Override_Method;

public class TheAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        builder.Advice.ImplementInterface( builder.Target, typeof(IDisposable), whenExists: OverrideStrategy.Override );
    }

    [InterfaceMember]
    public void Dispose()
    {
        meta.Proceed();
        Console.WriteLine( "TheAspect.Dispose()" );
    }
}

// <target>
[TheAspect]
internal class C : IDisposable
{
    public void Dispose()
    {
        Console.WriteLine( "C.Dispose()" );
    }
}