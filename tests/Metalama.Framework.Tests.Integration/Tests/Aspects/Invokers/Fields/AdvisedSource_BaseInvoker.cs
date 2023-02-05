using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Invokers;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedSource_BaseInvoker
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
                meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value =  value ;
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