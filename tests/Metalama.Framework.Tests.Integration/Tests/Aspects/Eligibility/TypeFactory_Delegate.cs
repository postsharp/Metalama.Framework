using System;
using System.Linq;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Eligibility.TypeFactory_Delegate;

class TestAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder) { }

    public override void BuildEligibility(IEligibilityBuilder<INamedType> builder)
    {
        builder.MustSatisfy(_ =>
        {
            TypeFactory.GetType(typeof(RunTimeClass));
            return true;
        }, _ => $"");
    }
}

class RunTimeClass { }

// <target>
[TestAspect]
class TargetClass
{
}