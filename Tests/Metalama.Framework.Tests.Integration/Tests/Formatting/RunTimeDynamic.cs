using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.RunTimeDynamic
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var x = meta.Target.Parameters[0].Value;
            Console.WriteLine(x);

            return meta.Proceed();
        }
    }

    class TargetCode
    {
        [Aspect]
        void M(int a) {}
    }



}