internal class TargetClass
{
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable.OverrideAttribute]
  public global::System.Int32 Field
  {
    get
    {
      global::System.Console.WriteLine("Override.");
      _ = this.Field_Source;
      return this.Field_Source;
    }
    set
    {
      global::System.Console.WriteLine("Override.");
      this.Field_Source = value;
      this.Field_Source = value;
    }
  }
  private global::System.Int32 Field_Source { get; set; }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable.OverrideAttribute]
  public global::System.Int32 StaticField
  {
    get
    {
      global::System.Console.WriteLine("Override.");
      _ = this.StaticField_Source;
      return this.StaticField_Source;
    }
    set
    {
      global::System.Console.WriteLine("Override.");
      this.StaticField_Source = value;
      this.StaticField_Source = value;
    }
  }
  private global::System.Int32 StaticField_Source { get; set; }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable.OverrideAttribute]
  public global::System.Int32 InitializerField
  {
    get
    {
      global::System.Console.WriteLine("Override.");
      _ = this.InitializerField_Source;
      return this.InitializerField_Source;
    }
    set
    {
      global::System.Console.WriteLine("Override.");
      this.InitializerField_Source = value;
      this.InitializerField_Source = value;
    }
  }
  private global::System.Int32 InitializerField_Source { get; set; } = 42;
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Uninlineable.OverrideAttribute]
  public global::System.Int32 ReadOnlyField
  {
    get
    {
      global::System.Console.WriteLine("Override.");
      _ = this.ReadOnlyField_Source;
      return this.ReadOnlyField_Source;
    }
    private init
    {
      global::System.Console.WriteLine("Override.");
      this.ReadOnlyField_Source = value;
      this.ReadOnlyField_Source = value;
    }
  }
  private global::System.Int32 ReadOnlyField_Source { get; init; }
  public TargetClass()
  {
    this.ReadOnlyField = 42;
  }
}