using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Eligibility.DeclarativeIntroduceOnInterface
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