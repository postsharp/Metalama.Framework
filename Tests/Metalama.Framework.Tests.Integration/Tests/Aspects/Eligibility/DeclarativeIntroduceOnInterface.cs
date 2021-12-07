using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Eligibility.DeclarativeIntroduceOnInterface
{
    class Aspect : TypeAspect
    {
        [Introduce]
        public void M() {}
       
    }

    [Aspect]
    interface TargetCode
    {
        
    }
}