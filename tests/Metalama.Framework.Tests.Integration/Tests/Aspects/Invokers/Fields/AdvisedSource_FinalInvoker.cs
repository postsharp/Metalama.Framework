using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Invokers;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedSource_FinalInvoker
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                return meta.Target.FieldOrProperty.With( InvokerOptions.Final ).Value;
            }

            set
            {
                meta.Target.FieldOrProperty.With( InvokerOptions.Final ).Value = value;
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Test]
        public int Field;
    }
}