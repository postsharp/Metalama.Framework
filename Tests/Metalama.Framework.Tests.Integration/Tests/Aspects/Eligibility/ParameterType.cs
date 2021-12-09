using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Eligibility.ParameterType
{
    class Aspect : OverrideMethodAspect
    {

        public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {
            base.BuildEligibility(builder);
            builder.Parameter(0).Type().MustBe<string>();
        }
 
        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException();
        }
    }

    class TargetCode
    {
        [Aspect]
        int Method(int a)
        {
            a= 0;
            return a;
        }
    }
}