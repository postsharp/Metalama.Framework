using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedSource_BaseInvoker
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                return meta.Target.FieldOrProperty.GetValue( meta.Base );
            }

            set
            {
                meta.Target.FieldOrProperty.SetValue( meta.Base, value );
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