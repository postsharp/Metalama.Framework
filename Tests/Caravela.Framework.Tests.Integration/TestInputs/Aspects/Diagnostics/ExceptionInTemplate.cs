using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.ExceptionInTemplate
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            var a = meta.CompileTime(0);
            var b = 1 / a;
            return meta.Proceed();
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