using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedSource_BaseInvoker
{
    public class TestAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic value)
        {
            meta.Event.Invokers.Base!.Add(meta.This, value);
        }

        public override void OverrideRemove(dynamic value)
        {
            meta.Event.Invokers.Base!.Remove(meta.This, value);
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
