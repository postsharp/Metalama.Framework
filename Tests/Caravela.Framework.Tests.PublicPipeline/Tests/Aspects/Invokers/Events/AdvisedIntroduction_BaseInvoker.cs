// @Skipped(#29134 - Invokers.Base is null for an override aspect applied to a field)

using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_BaseInvoker;
using Caravela.TestFramework;

[assembly: AspectOrder(typeof(TestAttribute), typeof(TestIntroductionAttribute))]

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_BaseInvoker
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestIntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public event EventHandler? Event;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TestAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.OverrideEventAccessors(builder.Target.Events.OfName(nameof(TestIntroductionAttribute.Event)).Single(), nameof(AddTemplate), nameof(RemoveTemplate), null);
        }

        [Template]
        public void AddTemplate(dynamic handler)
        {
            meta.Target.Event.Invokers.Base!.Add(meta.This, handler);
        }

        [Template]
        public void RemoveTemplate(dynamic handler)
        {
            meta.Target.Event.Invokers.Base!.Remove(meta.This, handler);
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass
    {
    }
}
