using System;
using Metalama.Framework.Aspects;

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