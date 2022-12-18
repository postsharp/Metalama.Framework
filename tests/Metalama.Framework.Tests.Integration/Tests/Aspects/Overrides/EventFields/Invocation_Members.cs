﻿using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Members;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Code;

#pragma warning disable CS0067

[assembly: AspectOrder(typeof(OverrideAttribute), typeof(IntroductionAttribute))]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_Members
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
            meta.This.IntroducedEvent?.Invoke(meta.This, new EventArgs());
            meta.ThisType.IntroducedStaticEvent?.Invoke(meta.This, new EventArgs());
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
            this.Event?.Invoke(this, new EventArgs());
            StaticEvent?.Invoke(this, new EventArgs());
        }
    }
}