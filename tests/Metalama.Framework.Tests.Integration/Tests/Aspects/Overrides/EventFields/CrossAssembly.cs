using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.EventFields.CrossAssembly
{
    // <target>
    [Override]
    [Introduction]
    internal class TargetClass
    {
        public event EventHandler? ExistingEvent;

        public event EventHandler? ExistingEvent_Initializer = Foo;

        public static void Foo(object? sender, EventArgs args)
        {
        }
    }
}