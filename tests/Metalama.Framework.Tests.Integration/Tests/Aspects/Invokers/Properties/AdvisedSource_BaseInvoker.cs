using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedSource_BaseInvoker
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                return meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value;
            }

            set
            {
                meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value = ( value );
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Test]
        public int Property { get; set; }
    }
}
