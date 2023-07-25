using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Eligibility.NoStruct
{
    class Aspect : OverrideMethodAspect
    {

        public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {
            base.BuildEligibility(builder);
            // TODO - there's a compilation error if Metalama.Framework.Code.TypeKind is not fully specified.
            builder.DeclaringType().MustSatisfy( t => t.TypeKind != Metalama.Framework.Code.TypeKind.Struct, t => $"{t} cannot be a struct" );
        }
 
        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }

    struct TargetCode
    {
        [Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}