﻿using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using Metalama.Framework.Code;
using System.Linq;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_ContainingExpressionBodies
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach(var @event in builder.Target.Events.Where( e => e.Name is "Event" or "StaticEvent" ) )
            {
                builder.Advise.OverrideAccessors(@event, nameof(OverrideAdd), nameof(OverrideRemove));
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

    // <target>
    [Override]
    internal class TargetClass
    {
        public event EventHandler? Event;
        public static event EventHandler? StaticEvent;

        static TargetClass() => StaticEvent?.Invoke(null, new EventArgs());

        public TargetClass() => this.Event?.Invoke(this, new EventArgs());

        ~TargetClass() => this.Event?.Invoke(this, new EventArgs());

        public void Foo() => this.Event?.Invoke(this, new EventArgs());

        public static void Bar() => StaticEvent?.Invoke(null, new EventArgs());

        public int Baz
        {
            init => this.Event?.Invoke(this, new EventArgs());
        }

        public event EventHandler? Quz
        {
            add => this.Event?.Invoke(this, new EventArgs());
            remove => this.Event?.Invoke(this, new EventArgs());
        }

        public int this[int index]
        {
            set => this.Event?.Invoke(this, new EventArgs());
        }
    }
}