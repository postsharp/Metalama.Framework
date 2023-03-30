[DeepClone]
internal class AutomaticallyCloneable : global::System.ICloneable
{
  private int _a;
  private ManuallyCloneable? _b;
  private AutomaticallyCloneable? _c;
  public virtual global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.AutomaticallyCloneable Clone()
  {
    var clone = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.AutomaticallyCloneable)base.MemberwiseClone());
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.AutomaticallyCloneable)clone)._b = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.ManuallyCloneable?)this._b?.Clone()!);
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.AutomaticallyCloneable)clone)._c = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.AutomaticallyCloneable?)this._c?.Clone()!);
    return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.AutomaticallyCloneable)clone;
  }
  global::System.Object global::System.ICloneable.Clone()
  {
    return (global::System.Object)this.Clone();
  }
}
internal class DerivedCloneable : AutomaticallyCloneable
{
  private string? _d;
  public override global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.DerivedCloneable Clone()
  {
    var clone = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.DerivedCloneable)base.Clone());
    return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2.DerivedCloneable)clone;
  }
}