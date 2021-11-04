using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Eligibility.NoStruct
{
    class Aspect : OverrideMethodAspect
    {

        public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {
            base.BuildEligibility(builder);
            // TODO - there's a compilation error if Caravela.Framework.Code.TypeKind is not fully specified.
            builder.DeclaringType().MustSatisfy( t => t.TypeKind != Caravela.Framework.Code.TypeKind.Struct, t => $"{t} cannot be a struct" );
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