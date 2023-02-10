[Before]
[Test]
[After]
public class Target
{
  private global::System.Int32 _field;
  public global::System.Int32 Field
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this._field;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this._field = value;
      return;
    }
  }
  private static global::System.Int32 _field_Static;
  public static global::System.Int32 Field_Static
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target._field_Static;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target._field_Static = value;
      return;
    }
  }
  private global::System.Int32 _introducedField;
  public global::System.Int32 IntroducedField
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this._introducedField;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this._introducedField = value;
      return;
    }
  }
  private global::System.Int32 _introducedField_Static;
  public global::System.Int32 IntroducedField_Static
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this._introducedField_Static;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this._introducedField_Static = value;
      return;
    }
  }
  public void Introduced()
  {
    _ = this._field;
    this._field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target._field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target._field_Static = 42;
  }
  public void IntroducedAfter()
  {
    _ = this.Field;
    this.Field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target.Field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target.Field_Static = 42;
    _ = this.IntroducedField;
    this.IntroducedField = 42;
    _ = this.IntroducedField_Static;
    this.IntroducedField_Static = 42;
  }
  public void IntroducedBefore()
  {
    _ = this._field;
    this._field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target._field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker.Target._field_Static = 42;
  }
}