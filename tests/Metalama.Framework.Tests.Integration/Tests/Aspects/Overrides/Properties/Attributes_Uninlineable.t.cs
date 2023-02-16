[Introduction]
[Override]
internal class TargetClass
{
  [FieldOnly]
  [FieldAndProperty]
  private int _autoProperty;
  [FieldAndProperty]
  [PropertyOnly]
  public int AutoProperty
  {
    [MethodOnly]
    [return: ReturnValueOnly]
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      _ = this._autoProperty;
      return this._autoProperty;
    }
    [MethodOnly]
    [param: ParamOnly]
    [return: ReturnValueOnly]
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      this._autoProperty = value;
      this._autoProperty = value;
    }
  }
  [FieldAndProperty]
  [PropertyOnly]
  public int Property
  {
    [MethodOnly]
    [return: ReturnValueOnly]
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      _ = this.Property_Source;
      return this.Property_Source;
    }
    [MethodOnly]
    [param: ParamOnly]
    [return: ReturnValueOnly]
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      this.Property_Source = value;
      this.Property_Source = value;
    }
  }
  private int Property_Source
  {
    get
    {
      Console.WriteLine("Original Property");
      return 42;
    }
    set
    {
      Console.WriteLine("Original Property");
    }
  }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.FieldOnlyAttribute]
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.FieldAndPropertyAttribute]
  private global::System.Int32 _introducedAutoProperty;
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.FieldAndPropertyAttribute]
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.PropertyOnlyAttribute]
  public global::System.Int32 IntroducedAutoProperty
  {
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.MethodOnlyAttribute]
    [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.ReturnValueOnlyAttribute]
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      _ = this._introducedAutoProperty;
      return this._introducedAutoProperty;
    }
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.MethodOnlyAttribute]
    [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.ReturnValueOnlyAttribute]
    [param: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.ParamOnlyAttribute]
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      this._introducedAutoProperty = value;
      this._introducedAutoProperty = value;
    }
  }
  private global::System.Int32 IntroducedProperty_Introduction
  {
    get
    {
      global::System.Console.WriteLine("Original Property");
      return default(global::System.Int32);
    }
    set
    {
      global::System.Console.WriteLine("Original Property");
    }
  }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.FieldAndPropertyAttribute]
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.PropertyOnlyAttribute]
  public global::System.Int32 IntroducedProperty
  {
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.MethodOnlyAttribute]
    [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.ReturnValueOnlyAttribute]
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      _ = this.IntroducedProperty_Introduction;
      return this.IntroducedProperty_Introduction;
    }
    [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.MethodOnlyAttribute]
    [return: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.ReturnValueOnlyAttribute]
    [param: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Attributes_Uninlineable.ParamOnlyAttribute]
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      this.IntroducedProperty_Introduction = value;
      this.IntroducedProperty_Introduction = value;
    }
  }
}