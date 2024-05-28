#if TEST_OPTIONS
// @Include(_AdviceResultShared.cs)
# endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.AdviceResult_Override_MembersFail
{
    /*
     * Tests that advice result with members with ignore override strategy contains correct values.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            var result = aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface), OverrideStrategy.Override );

            if (result.Outcome != Advising.AdviceOutcome.Error)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Error.");
            }

            if (result.AdviceKind != Advising.AdviceKind.ImplementInterface)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of ImplementInterface.");
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Fail)]
        public void BaseMethod()
        {
        }

        [Introduce(WhenExists = OverrideStrategy.Fail)]
        public int BaseProperty { get; set; }

        [Introduce(WhenExists = OverrideStrategy.Fail)]
        public event EventHandler? BaseEvent;

        [Introduce(WhenExists = OverrideStrategy.Fail)]
        public void Method()
        {
        }

        [Introduce(WhenExists = OverrideStrategy.Fail)]
        public int Property { get; set; }

        [Introduce(WhenExists = OverrideStrategy.Fail)]
        public event EventHandler? Event;
    }

    // <target>
    [Introduction]
    public class TargetClass
    {
        public void BaseMethod()
        {
        }

        public int BaseProperty { get; set; }


        public event EventHandler? BaseEvent;

        public void Method()
        {
        }

        public int Property { get; set; }


        public event EventHandler? Event;
    }
}