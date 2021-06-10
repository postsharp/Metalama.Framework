using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Initialize.Tags
{
    class Aspect : OverrideMethodAspect
    {
        public override void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.Tags.Add("Friend", "Bernard");
            base.Initialize(aspectBuilder);
        }

        public override dynamic OverrideMethod() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }

    class TargetCode
    {
        [Aspect]
        int Method(int a)
        {
            global::System.Console.WriteLine("Bernard");
            return a;
        }
    }
}
