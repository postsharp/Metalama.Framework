﻿// @Skipped(#28883)

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedSource_FinalInvoker
{
    public class TestAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic handler)
        {
            meta.Event.Invokers.Final!.AddDelegate(meta.This, handler);
        }

        public override void OverrideRemove(dynamic handler)
        {
            meta.Event.Invokers.Final!.RemoveDelegate(meta.This, handler);
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
