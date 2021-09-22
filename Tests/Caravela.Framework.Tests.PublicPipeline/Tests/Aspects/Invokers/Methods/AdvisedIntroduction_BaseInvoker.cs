// @Skipped(#29134 - Invokers.Base is null for an override aspect applied to a field)

using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedIntroduction_BaseInvoker;
using Caravela.TestFramework;

[assembly: AspectOrder(typeof(TestAttribute), typeof(TestIntroductionAttribute))]

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedIntroduction_BaseInvoker
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestIntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public int Method(int x)
        {
            return x;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TestAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.AdviceFactory.OverrideMethod(builder.Target.Methods.OfName(nameof(TestIntroductionAttribute.Method)).Single(), nameof(MethodTemplate));
        }

        [Template]
        public dynamic MethodTemplate()
        {
            return meta.Target.Method.Invokers.Base!.Invoke(meta.This, meta.Target.Method.Parameters[0].Value);
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass
    {
    }
}
