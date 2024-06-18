#if TEST_OPTIONS
// In C# 10, we need to generate slightly different code.
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
# endif

using Metalama.Framework.Aspects;
using System;

#pragma warning disable CS0067, CS0414

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.Struct_Declarative
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? IntroducedEvent;

        [Introduce]
        public event EventHandler? IntroducedEvent_Initializer = Foo;

        [Introduce]
        public event EventHandler? IntroducedEvent_Static;

        [Introduce]
        public event EventHandler? IntroducedEvent_Static_Initializer = Foo;

        [Introduce]
        public static void Foo( object? sender, EventArgs args ) { }
    }

    // <target>
    [Introduction]
    internal struct TargetStruct
    {
        private int _existingField;

        public TargetStruct( int x )
        {
            _existingField = x;
        }
    }
}