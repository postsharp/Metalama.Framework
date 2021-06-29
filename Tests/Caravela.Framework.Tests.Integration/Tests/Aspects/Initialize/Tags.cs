using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Initialize.Tags
{
    class Aspect : Attribute, IAspect<IMethod>
    {
        public void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.AdviceFactory.OverrideMethod(builder.TargetDeclaration, nameof(OverrideMethod), new () { {"Friend", "Bernard" } });
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
        // <target>
        [Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}