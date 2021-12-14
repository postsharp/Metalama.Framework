using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Inheritance.InheritedPropertyAttribute
{
    internal class Aspect : OverrideFieldOrPropertyAspect, IInheritedAspect
    {
        public override dynamic? OverrideProperty
        {
            get => meta.Proceed() + 1;
            set => meta.Target.FieldOrProperty.Value = value - 1;
        }
    }

    // <target>
    internal class Targets
    {
        private class BaseClass
        {
            [Aspect]
            public virtual int P { get; set; }
        }

        private class DerivedClass : BaseClass
        {
            public override int P { get; set; }
        }
    }
}