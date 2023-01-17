using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_Value;

[assembly: AspectOrder( typeof(OverrideAttribute), typeof(TestIntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_Value
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestIntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? Event;
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class OverrideAttribute : TypeAspect
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
        public void AddTemplate( dynamic value )
        {
            Console.WriteLine("Override");
            meta.Target.Event.AddMethod.Invoke( value );
        }

        [Template]
        public void RemoveTemplate( dynamic value )
        {
            Console.WriteLine("Override");
            meta.Target.Event.RemoveMethod.Invoke( value );
        }
    }

    // <target>
    [TestIntroduction]
    [Override]
    internal class TargetClass { }
}