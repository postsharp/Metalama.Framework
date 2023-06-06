#if TEST_OPTIONS
// @Include(_AdviceResultShared.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.AdviceResult_Fail_ImplementedBase
{
    /*
     * Tests that advice result with fail override strategy contains correct values.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            var result = aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface), OverrideStrategy.Fail );

            if (result.Outcome != Advising.AdviceOutcome.Error)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Error.");
            }

            if (result.AdviceKind != Advising.AdviceKind.ImplementInterface)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of ImplementInterface.");
            }

            if (result.InterfaceMembers.Count != 0)
            {
                throw new InvalidOperationException($"InterfaceMembers collection was not empty.");
            }
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Default)]
        public void BaseMethod()
        {
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Default)]
        public int BaseProperty { get; set; }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Default)]
        public event EventHandler? BaseEvent;

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Default)]
        public void Method()
        {
        }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Default)]
        public int Property { get; set; }

        [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Default)]
        public event EventHandler? Event;
    }

    // <target>
    [Introduction]
    public class TargetClass : IBaseInterface
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