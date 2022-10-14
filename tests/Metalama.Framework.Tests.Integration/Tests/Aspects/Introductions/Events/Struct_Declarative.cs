using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Events.Struct_Declarative
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? IntroducedEvent;

        //[Introduce]
        //public event EventHandler? IntroducedEvent_Initializer = Foo;

        [Introduce]
        public event EventHandler? IntroducedEvent_Static;

        //[Introduce]
        //public event EventHandler? IntroducedEvent_Static_Initializer = Foo;

        public static void Foo(object sender, EventArgs args)
        {
        }
    }

    // <target>
    [Introduction]
    internal struct TargetStruct
    {
        private int _existingField;

        public TargetStruct(int x)
        {
            this._existingField = x;
        }
    }
}