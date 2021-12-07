using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug29495
{
#pragma warning disable CS0067
    class Aspect : OverrideMethodAspect
    {
        public MyEnum Value {get; set; }

        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

    class TargetCode
    {
        [Aspect( Value = MyEnum.B )]
        int Method(int a)
{
    global::System.Console.WriteLine("B");
            return a;
}
    }
}