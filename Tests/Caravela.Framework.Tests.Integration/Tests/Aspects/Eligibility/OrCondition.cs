using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Eligibility.OrCondition
{
    class Aspect : OverrideMethodAspect
    {

        public override void BuildEligibility(IEligibilityBuilder<IMethod> builder)
        {
            base.BuildEligibility(builder);
            builder.MustSatisfyAny( 
            b => b.MustSatisfy( x => x.IsVirtual, x => $"{x} must be virtual"),
            b => b.MustSatisfy( x => x.IsAbstract, x => $"{x} must be abstract") );
            
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
            return a;
        }
    }
}