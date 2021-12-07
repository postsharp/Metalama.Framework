using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Overrides.Methods.ConfigureAfterBuild2
{
    class Aspect : OverrideMethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            this.UseEnumerableTemplateForAnyEnumerable = true;
        }

        public override dynamic? OverrideMethod() => null;
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