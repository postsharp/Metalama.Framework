using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.Tests.Integration.Aspects.Initialize.Tags
{
    class Aspect : Attribute, IAspect<IMethod>
    {
        public void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            var options = AdviceOptions.Default.AddTag("Friend", "Bernard");
            builder.AdviceFactory.OverrideMethod(builder.TargetDeclaration, nameof(OverrideMethod), options);
        }

        
        [Template]
        private dynamic OverrideMethod()
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