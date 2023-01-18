﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Events.CallerAttributes_Inlined
{
    /*
     * Tests that overriding event does not transform caller attribute method invocations when the source is inlined.
     */

    public class OverrideAttribute : EventAspect
    {
        public override void BuildAspect(IAspectBuilder<IEvent> builder)
        {
            builder.Advice.OverrideAccessors(builder.Target, nameof(Override), nameof(Override));
        }

        [Template]
        public dynamic? Override()
        {
            Console.WriteLine("This is the overridden method.");
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public event EventHandler OverriddenEvent
        {
            add
            {
                this.MethodWithCallerMemberName(42);
                this.MethodWithCallerMemberName(42, y: 27);
                this.MethodWithCallerMemberName(42, name1: "foo", y: 27);
                this.MethodWithCallerMemberName(42, "foo", 27);
                this.MethodWithCallerMemberName(42, "foo", 27, "bar");
            }

            remove
            {
                this.MethodWithCallerMemberName(42);
                this.MethodWithCallerMemberName(42, y: 27);
                this.MethodWithCallerMemberName(42, name1: "foo", y: 27);
                this.MethodWithCallerMemberName(42, "foo", 27);
                this.MethodWithCallerMemberName(42, "foo", 27, "bar");
            }
        }

        public void MethodWithCallerMemberName(int x, [CallerMemberName]string name1 = "", int y = 0, [CallerMemberName] string name2 = "")
        {
        }
    }
}