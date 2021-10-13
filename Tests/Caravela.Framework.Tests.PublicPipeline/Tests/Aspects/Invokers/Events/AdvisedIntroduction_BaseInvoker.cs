﻿// @Skipped(#29134 - Invokers.Base is null for an override aspect applied to a field)

using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_BaseInvoker;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedIntroduction_BaseInvoker
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
        public void AddTemplate( dynamic value )
        {
            meta.Target.Event.Invokers.Base!.Add( meta.This, value );
        }

        [Template]
        public void RemoveTemplate( dynamic value )
        {
            meta.Target.Event.Invokers.Base!.Remove( meta.This, value );
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}