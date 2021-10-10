using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_Value;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_Value
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestIntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? Event;
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.OverrideEventAccessors(
                builder.Target.Events.OfName( nameof(TestIntroductionAttribute.Event) ).Single(),
                nameof(AddTemplate),
                nameof(RemoveTemplate),
                null );
        }

        [Template]
        public void AddTemplate( dynamic handler )
        {
            meta.Target.Event.AddMethod.Invoke( handler );
        }

        [Template]
        public void RemoveTemplate( dynamic handler )
        {
            meta.Target.Event.RemoveMethod.Invoke( handler );
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}