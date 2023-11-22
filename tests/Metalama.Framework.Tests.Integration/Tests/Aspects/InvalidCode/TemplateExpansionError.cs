using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.InvalidCode.TemplateExpansionError
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            meta.CompileTime(this.CompileTimeExceptionMethod());
            
            return default;
        }

        private int CompileTimeExceptionMethod()
        {
            throw new Exception();
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