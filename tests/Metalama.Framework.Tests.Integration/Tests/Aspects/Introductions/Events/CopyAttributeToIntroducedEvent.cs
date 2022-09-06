#pragma warning disable CS0067

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.CopyAttributeToIntroducedEvent
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [Foo]
        public event EventHandler? Event
        {
            [return: Foo]
            [param: Foo]
            add
            {
                Console.WriteLine( "Original add accessor." );
            }

            [return: Foo]
            [param: Foo]
            remove
            {
                Console.WriteLine( "Original remove accessor." );
            }
        }

        [Introduce]
        [Foo]
        public event EventHandler? FieldLikeEvent;
    }

    public class FooAttribute : Attribute { }

    // <target>
    [Introduction]
    internal class TargetClass { }
}