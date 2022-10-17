using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;
using System.Collections.Generic;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Simple
{
    public class OverrideAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic value)
        {
            Console.WriteLine("This is the add template.");
            meta.Proceed();
        }

        public override void OverrideRemove(dynamic value)
        {
            Console.WriteLine("This is the remove template.");
            meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public event EventHandler? Event;

        [Override]
        public event EventHandler? Event_Initializer = Foo;

        private static void Foo(object? sender, EventArgs args)
        {
        }
    }
}