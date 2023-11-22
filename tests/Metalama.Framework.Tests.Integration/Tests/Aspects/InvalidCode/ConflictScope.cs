using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.InvalidCode.ConflictScope
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
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