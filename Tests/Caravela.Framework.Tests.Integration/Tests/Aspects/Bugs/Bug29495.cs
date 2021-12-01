using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug29495
{
    class Aspect : OverrideMethodAspect
    {
        public MyEnum Value {get; set; }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(this.Value.ToString());
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        [Aspect( Value = MyEnum.B )]
        int Method(int a)
        {
            return a;
        }
    }
}