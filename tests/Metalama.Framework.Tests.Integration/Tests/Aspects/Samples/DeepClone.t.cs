internal class Targets
    {
        [DeepClone]
        private class AutomaticallyCloneable:global::System.ICloneable        {
            private int a;

            private ManuallyCloneable? b;

            private AutomaticallyCloneable? c;


public virtual global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable Clone()
{
    var clone = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)base.MemberwiseClone());
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)clone).b = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.ManuallyCloneable? )((global::System.ICloneable)this.b).Clone());
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)clone).c = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable? )((global::System.ICloneable)this.c).Clone());
    return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable)clone;
}
private global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable Clone_Source()
{
    return default(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.AutomaticallyCloneable);
}

global::System.Object global::System.ICloneable.Clone()
{
    return (global::System.Object)this.Clone();
}        }

        [DeepClone]
        private class Derived : AutomaticallyCloneable
        {
            private string d;


public override global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived Clone()
{
    var clone = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived)base.Clone());
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived)clone).d = ((global::System.String)((global::System.ICloneable)this.d).Clone());
    return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Targets.Derived)clone;
}        }
    }