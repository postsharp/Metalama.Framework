[Introduction]
[Override]
internal class TargetClass
{
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Attributes_Uninlineable.FieldAndPropertyAttribute]
  public global::System.Int32 Field
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      _ = this.Field_Source;
      return this.Field_Source;
    }
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      this.Field_Source = value;
      this.Field_Source = value;
    }
  }
  [field: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Attributes_Uninlineable.FieldOnlyAttribute]
  private global::System.Int32 Field_Source { get; set; }
  [global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Attributes_Uninlineable.FieldAndPropertyAttribute]
  public global::System.Int32 IntroducedField
  {
    get
    {
      global::System.Console.WriteLine("This is the overridden getter.");
      _ = this.IntroducedField_Source;
      return this.IntroducedField_Source;
    }
    set
    {
      global::System.Console.WriteLine("This is the overridden setter.");
      this.IntroducedField_Source = value;
      this.IntroducedField_Source = value;
    }
  }
  [field: global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.Attributes_Uninlineable.FieldOnlyAttribute]
  private global::System.Int32 IntroducedField_Source { get; set; }
}