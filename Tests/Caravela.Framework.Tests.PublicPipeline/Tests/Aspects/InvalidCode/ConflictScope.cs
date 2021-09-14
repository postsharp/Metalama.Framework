using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.InvalidCode.ConflictScope
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Dictionary<IMethod,TargetCode> conflict = new();
            
            return default;
        }
    
    }

    class TargetCode
    {
        // <target>
        [Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}