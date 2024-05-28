﻿#if TEST_OPTIONS
// @Include(_AdviceResultShared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Engine.Advising;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.AdviceResult_Fail_Implemented
{
    /*
     * Tests that advice result with fail override strategy contains correct values.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            var result = aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface), OverrideStrategy.Fail );

            if (result.Outcome != AdviceOutcome.Error)
            {
                throw new InvalidOperationException( $"Outcome was {result.Outcome} instead of Error." );
            }

            if (result.AdviceKind != AdviceKind.ImplementInterface)
            {
                throw new InvalidOperationException( $"AdviceKind was {result.AdviceKind} instead of ImplementInterface." );
            }

            if (result.GetObsoleteInterfaceMembers().Count != 0)
            {
                throw new InvalidOperationException( $"InterfaceMembers collection was not empty." );
            }

            aspectBuilder.Advice.WithTemplateProvider(new AdviceResultTemplates())
                .Override(
                    aspectBuilder.Target.Methods.OfName("Witness").Single(),
                    nameof(AdviceResultTemplates.WitnessTemplate),
                    args: new { types = result.Interfaces, members = result.GetObsoleteInterfaceMembers() } );
        }

        [Introduce]
        public void BaseMethod() { }

        [Introduce]
        public int BaseProperty { get; set; }

        [Introduce]
        public event EventHandler? BaseEvent;

        [Introduce]
        public void Method() { }

        [Introduce]
        public int Property { get; set; }

        [Introduce]
        public event EventHandler? Event;
    }

    // <target>
    [Introduction]
    public class TargetClass : IInterface
    {
        public void BaseMethod() { }

        public int BaseProperty { get; set; }

        public event EventHandler? BaseEvent;

        public void Method() { }

        public int Property { get; set; }

        public event EventHandler? Event;

        public void Witness() { }
    }
}