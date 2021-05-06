using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.ExceptionInTemplate
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            var a = compileTime(0);
            var b = 1 / a;
            return proceed();
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