[EnumViewModel]
public class TargetClass
{
  [Flags]
  public enum StringOptions
  {
    None,
    ToUpperCase = 1,
    RemoveSpace = 2,
    Trim = 4
  }
  public class StringOptionsViewModel : global::System.Object
  {
    private readonly global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions _value;
    public StringOptionsViewModel(global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions value)
    {
      this._value = value;
    }
    public global::System.Boolean IsNone
    {
      get
      {
        return (global::System.Boolean)((this._value & global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.None) == global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.None);
      }
    }
    public global::System.Boolean IsRemoveSpace
    {
      get
      {
        return (global::System.Boolean)((this._value & global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.RemoveSpace) == global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.RemoveSpace);
      }
    }
    public global::System.Boolean IsToUpperCase
    {
      get
      {
        return (global::System.Boolean)((this._value & global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.ToUpperCase) == global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.ToUpperCase);
      }
    }
    public global::System.Boolean IsTrim
    {
      get
      {
        return (global::System.Boolean)((this._value & global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.Trim) == global::Metalama.Framework.Tests.Integration.Aspects.Samples.EnumViewModel2.TargetClass.StringOptions.Trim);
      }
    }
  }
}