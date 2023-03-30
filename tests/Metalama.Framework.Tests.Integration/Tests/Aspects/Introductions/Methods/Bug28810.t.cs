internal class Targets
{
  private class NaturallyCloneable : ICloneable
  {
    public object Clone()
    {
      return new NaturallyCloneable();
    }
  }
  [DeepClone]
  private class BaseClass : global::System.ICloneable
  {
    private int a;
    private NaturallyCloneable? b;
    public virtual global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28810.Targets.BaseClass Clone()
    {
      return null;
    }
    global::System.Object global::System.ICloneable.Clone()
    {
      return (global::System.Object)this.Clone();
    }
  }
}