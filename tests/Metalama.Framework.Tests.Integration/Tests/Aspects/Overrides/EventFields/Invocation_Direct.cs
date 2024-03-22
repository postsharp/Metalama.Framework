using Metalama.Framework.Aspects;
using System;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Direct;
using Metalama.Framework.Code;

#pragma warning disable CS0067

[assembly: AspectOrder(typeof(OverrideAttribute), typeof(IntroductionAttribute))]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Direct
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach(var @event in builder.Target.Events)
            {
                builder.Advice.OverrideAccessors(@event, nameof(OverrideAdd), nameof(OverrideRemove));
            }
        }

        [Template]
        public void OverrideAdd(dynamic value)
        {
            Console.WriteLine("This is the add template.");
            meta.Proceed();
        }

        [Template]
        public void OverrideRemove(dynamic value)
        {
            Console.WriteLine("This is the remove template.");
            meta.Proceed();
        }
    }

    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? IntroducedEvent;

        [Introduce]
        public static event EventHandler? IntroducedStaticEvent;

        [Introduce]
        public void Bar()
        {
            if (meta.This.IntroducedEvent != null)
            {
                meta.This.IntroducedEvent(meta.This, new EventArgs());
            }

            if (meta.ThisType.IntroducedStaticEvent != null)
            {
                meta.ThisType.IntroducedStaticEvent(meta.This, new EventArgs());
            }
        }
    }

    // <target>
    [Override]
    [Introduction]
    internal class TargetClass
    {
        public event EventHandler? Event;
        public static event EventHandler? StaticEvent;

        public void Foo()
        {
            if (this.Event != null)
            {
                this.Event(this, new EventArgs());
            }

            if (StaticEvent != null)
            {
                StaticEvent(this, new EventArgs());
            }
        }
    }
}