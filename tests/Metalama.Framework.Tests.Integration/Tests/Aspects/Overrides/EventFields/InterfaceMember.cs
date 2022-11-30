﻿using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;
using System;
using System.Collections.Generic;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Code;

#pragma warning disable CS0067

[assembly: AspectOrder(typeof(OverrideAttribute), typeof(IntroductionAttribute))]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.InterfaceMember
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var @event in builder.Target.Events)
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

    public interface Interface
    {
        public event EventHandler? Event;

        public event EventHandler? InitializerEvent;

    }

    public interface IntroducedInterface
    {
        public event EventHandler? IntroducedEvent;

        public event EventHandler? InitializerIntroducedEvent;

        public event EventHandler? ExplicitIntroducedEvent;

        public event EventHandler? InitializerExplicitIntroducedEvent;
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.ImplementInterface(builder.Target, typeof(IntroducedInterface));
        }

        [InterfaceMember(IsExplicit = false)]
        public event EventHandler? IntroducedEvent;

        [InterfaceMember(IsExplicit = false)]
        public event EventHandler? InitializerIntroducedEvent = Bar;

        [InterfaceMember(IsExplicit = true)]
        public event EventHandler? ExplicitIntroducedEvent;

        [InterfaceMember(IsExplicit = true)]
        public event EventHandler? InitializerExplicitIntroducedEvent = Bar;

        [Introduce]
        public static void Bar(object? sender, EventArgs args)
        {
        }
    }

    // <target>
    [Override]
    [Introduction]
    internal class TargetClass : Interface
    {
        public event EventHandler? Event;

        public event EventHandler? InitializerEvent = Foo;

        public static void Foo(object? sender, EventArgs args)
        {
        }
    }
}