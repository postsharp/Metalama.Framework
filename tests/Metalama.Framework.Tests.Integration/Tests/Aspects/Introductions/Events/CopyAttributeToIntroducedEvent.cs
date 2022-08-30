#pragma warning disable CS0067

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent;

[assembly: AspectOrder( typeof(IntroduceAttribute), typeof(OverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [Override]
        public event EventHandler? Event
        {
            add
            {
                Console.WriteLine( "Original add accessor." );
            }

            remove
            {
                Console.WriteLine( "Original remove accessor." );
            }
        }
    }

    public class OverrideAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic value)
        {
            Console.WriteLine("This is the overriden add template.");
            meta.Proceed();
        }

        public override void OverrideRemove(dynamic value)
        {
            Console.WriteLine("This is the overriden remove template.");
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}