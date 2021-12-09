// @Skipped #28884

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedSource_Value
{
    public class TestAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic handler)
        {
            meta.Target.Event.AddMethod.Invoke( handler );
        }

        public override void OverrideRemove(dynamic handler)
        {
            meta.Target.Event.RemoveMethod.Invoke( handler );
        }
    }

    // <target>
    internal class TargetClass
    {
        private EventHandler? _field;

        [Test]
        public event EventHandler Event
        {
            add => this._field += value;
            remove => this._field -= value;
        }

        [Test]
        public event EventHandler? EventField;
    }
}
