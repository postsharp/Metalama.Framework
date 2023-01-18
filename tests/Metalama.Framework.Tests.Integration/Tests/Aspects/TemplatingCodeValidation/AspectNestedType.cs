﻿using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.AspectNestedType;

[CompileTime]
class OuterClass
{
    public class MyAspect : TypeAspect { }

    public class MyIAspect : IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder)
        {
        }
    }
}

[CompileTime]
interface OuterInterface
{
    public class MyAspect : Aspect { }
}

[CompileTime]
struct OuterStruct
{
    public class MyIAspect : IAspect { }
}