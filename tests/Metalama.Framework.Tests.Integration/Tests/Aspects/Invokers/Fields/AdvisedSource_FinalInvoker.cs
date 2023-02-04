using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedSource_FinalInvoker
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                return meta.Target.FieldOrProperty.GetValue( meta.This );
            }

            set
            {
                meta.Target.FieldOrProperty.SetValue( meta.This, value );
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