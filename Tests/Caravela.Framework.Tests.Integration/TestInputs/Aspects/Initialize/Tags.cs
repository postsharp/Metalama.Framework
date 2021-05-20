using System;
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
            
            // By design, values written after the creation of advices are not taken into account.
            aspectBuilder.Tags["Friend"] = "Julia";
            
        }

        public override dynamic OverrideMethod()
        {
            Console.WriteLine( (string?) meta.Tags["Friend"] );
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        [Aspect]
        [TestOutput]
        int Method(int a)
        {
            return a;
        }
    }
}