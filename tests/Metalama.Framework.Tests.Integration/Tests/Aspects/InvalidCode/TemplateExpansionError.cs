using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

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