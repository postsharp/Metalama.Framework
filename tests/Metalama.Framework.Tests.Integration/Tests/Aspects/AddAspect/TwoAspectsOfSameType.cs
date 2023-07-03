using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.TwoAspectsOfSameType
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine($"{meta.AspectInstance.SecondaryInstances.Length} other instance(s)");
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