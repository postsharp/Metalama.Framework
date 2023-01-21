using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedSource_Value
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                return meta.Target.FieldOrProperty.Value;
            }

            set
            {
                meta.Target.FieldOrProperty.Value = value;
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