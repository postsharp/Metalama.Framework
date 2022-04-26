using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_FinalInvoker;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_FinalInvoker
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
            builder.Advice.OverrideAccessors(
                builder.Target.Events.OfName( nameof(TestIntroductionAttribute.Event) ).Single(),
                nameof(AddTemplate),
                nameof(RemoveTemplate),
                null );
        }

        [Template]
        public void AddTemplate( dynamic handler )
        {
            meta.Target.Event.Invokers.Final!.Add( meta.This, handler );
        }

        [Template]
        public void RemoveTemplate( dynamic handler )
        {
            meta.Target.Event.Invokers.Final!.Remove( meta.This, handler );
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}