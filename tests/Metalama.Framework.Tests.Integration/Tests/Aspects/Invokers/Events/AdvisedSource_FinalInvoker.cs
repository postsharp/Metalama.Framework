using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Testing.AspectTesting;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Events.AdvisedSource_FinalInvoker
{
    public class TestAttribute : OverrideEventAspect
    {
        public override void OverrideAdd( dynamic value )
        {
            meta.Target.Event.With( InvokerOptions.Final ).Add( meta.Target.Parameters[0].Value );
        }

        public override void OverrideRemove( dynamic value )
        {
            meta.Target.Event.With( InvokerOptions.Final ).Remove( meta.Target.Parameters[0].Value );
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