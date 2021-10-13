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
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }

    class TargetCode
    {
        [Aspect, Aspect]
        int Method(int a)
{
    global::System.Console.WriteLine($"1 other instance(s)");
            return a;
}
    }
}
