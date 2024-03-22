using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Eligibility.TypeFactory_Delegate_Error;

class TestAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder) { }

    public override void BuildEligibility(IEligibilityBuilder<INamedType> builder)
    {
        builder.AddRules(_ =>
        {
            TypeFactory.GetType(typeof(RunTimeClass));
        });
    }
}

class RunTimeClass { }

// <target>
[TestAspect]
class TargetClass
{
}