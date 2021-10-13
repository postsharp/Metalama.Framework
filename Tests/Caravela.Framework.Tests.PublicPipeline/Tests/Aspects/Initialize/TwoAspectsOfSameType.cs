using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.TwoAspectsOfSameType
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine($"{meta.AspectInstance.OtherInstances.Length} other instance(s)");
            return meta.Proceed();
        }
    }

    // <target>
    class TargetCode
    {
        [Aspect, Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}