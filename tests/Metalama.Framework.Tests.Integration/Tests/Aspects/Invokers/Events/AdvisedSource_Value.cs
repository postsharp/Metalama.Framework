using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedSource_Value
{
    public class TestAttribute : OverrideEventAspect
    {
        public override void OverrideAdd( dynamic value)
        {
            Console.WriteLine("Override");
            meta.Target.Event.AddMethod.Invoke( value );
        }

        public override void OverrideRemove( dynamic value )
        {
            Console.WriteLine("Override");
            meta.Target.Event.RemoveMethod.Invoke( value );
        }
    }

    // <target>
    internal class TargetClass
    {
        private EventHandler? _field;

        [Test]
        public event EventHandler Event
        {
            add => _field += value;
            remove => _field -= value;
        }

        [Test]
        public event EventHandler? EventField;
    }
}