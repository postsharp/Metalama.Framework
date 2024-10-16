using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.IO;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.NullableConstraint;

class MyAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.ImplementInterface(typeof(I));
    }

    [Introduce]
    void M<T1, T2>()
        where T1 : class?
        where T2 : Stream?
    {
    }

    [InterfaceMember]
    public void IM<T1, T2>()
    {
    }
}


interface I
{
    void IM<T1, T2>()
        where T1 : class?
        where T2 : Stream?;
}

// <target>
[MyAspect]
class C
{
}