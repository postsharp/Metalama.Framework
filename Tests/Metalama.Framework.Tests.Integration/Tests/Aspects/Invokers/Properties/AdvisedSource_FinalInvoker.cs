using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedSource_FinalInvoker
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                return meta.Target.FieldOrProperty.Invokers.Final.GetValue( meta.This );
            }

            set
            {
                meta.Target.FieldOrProperty.Invokers.Final.SetValue( meta.This, value );
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
