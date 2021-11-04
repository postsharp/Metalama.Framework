using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.InvalidCode.TemplateExpansionError
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