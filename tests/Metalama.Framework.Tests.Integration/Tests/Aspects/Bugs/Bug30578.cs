using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Aspects.Bug30578
{
    // Apply [RunTimeOrCompileTime] here as a workaround.
    enum AspectConsructorParameterType
    {
    }

    class AspectWithParametrizedConstructor : Aspect, IAspect<INamedType>
    {
        public AspectWithParametrizedConstructor(AspectConsructorParameterType parameter)
        {
        }

        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder)
        {
        }
    }

    // <target>
    [AspectWithParametrizedConstructor(default)]
    class TargetCode
    {
    }
}
