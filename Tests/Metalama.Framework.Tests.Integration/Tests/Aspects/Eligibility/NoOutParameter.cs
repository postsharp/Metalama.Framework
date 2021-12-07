using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Eligibility.NoOutParameter
{
    class Aspect : OverrideMethodAspect
    {

        public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {
            base.BuildEligibility(builder);
            builder.MustNotHaveRefOrOutParameter();
        }
 
        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }

    class TargetCode
    {
        [Aspect]
        int Method(out int a)
        {
            a= 0;
            return a;
        }
    }
}