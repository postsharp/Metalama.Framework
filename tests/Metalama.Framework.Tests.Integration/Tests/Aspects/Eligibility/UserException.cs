using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Eligibility.UserException;

public class MyAspect : TypeAspect
{
    public override void BuildEligibility( IEligibilityBuilder<INamedType> builder )
    {
        builder.MustSatisfy( t => throw new Exception(), x => $"{x} must have an attribute of type MyAttribute" );
    }
}

[MyAspect]
public class Test { }