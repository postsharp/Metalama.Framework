// Warning CS8601 on `(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.ManuallyCloneable)this.B?.Clone()`: `Possible null reference assignment.`
// Warning CS8601 on `(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)this.C?.Clone()`: `Possible null reference assignment.`
// Warning CS8601 on `(global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.ManuallyCloneable)this.G?.Clone()`: `Possible null reference assignment.`
[DeepClone]
internal partial class AutomaticallyCloneable : global::System.ICloneable
{
    public int A;
    public ManuallyCloneable B;
    public AutomaticallyCloneable C;
    public NotCloneable D;
    public virtual global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable Clone()
    {
        var clone = (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)base.MemberwiseClone();
        clone.B = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.ManuallyCloneable)this.B?.Clone());
        clone.C = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)this.C?.Clone());
        return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.AutomaticallyCloneable)clone;
    }
    global::System.Object global::System.ICloneable.Clone()
    {
        return (global::System.Object)this.Clone();
    }
}
internal class ManuallyCloneable : ICloneable
{
    public int E;
    public object Clone()
    {
        return new ManuallyCloneable()
        { E = this.E };
    }
}
internal class NotCloneable
{
    public int F;
}
internal partial class Derived : AutomaticallyCloneable
{
    public ManuallyCloneable G { get; private set; }
    public override global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Derived Clone()
    {
        var clone = (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Derived)base.Clone();
        clone.G = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.ManuallyCloneable)this.G?.Clone());
        return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty.Derived)clone;
    }
}