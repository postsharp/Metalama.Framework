using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Overrides.Methods.ConfigureAfterBuild
{
    class Aspect : OverrideMethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
            this.UseAsyncTemplateForAnyAwaitable = true;
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