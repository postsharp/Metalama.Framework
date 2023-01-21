using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedSource_BaseInvoker
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                return meta.Target.FieldOrProperty.Invokers.Base!.GetValue( meta.This );
            }

            set
            {
                meta.Target.FieldOrProperty.Invokers.Base!.SetValue( meta.This, value );
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