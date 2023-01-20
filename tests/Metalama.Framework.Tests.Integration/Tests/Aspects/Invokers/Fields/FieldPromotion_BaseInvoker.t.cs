[Before]
[Override]
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
      return global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target._field_Static;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target._field_Static = value;
      return;
    }
  }
  public int Field_NoOverride;
  public static int Field_Static_NoOverride;
  public void Introduced()
  {
    _ = this._field;
    this._field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target._field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target._field_Static = 42;
    _ = this.Field_NoOverride;
    this.Field_NoOverride = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static_NoOverride;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static_NoOverride = 42;
  }
  public void IntroducedAfter()
  {
    _ = this.Field;
    this.Field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static = 42;
    _ = this.Field_NoOverride;
    this.Field_NoOverride = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static_NoOverride;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static_NoOverride = 42;
  }
  public void IntroducedBefore()
  {
    _ = this._field;
    this._field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target._field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target._field_Static = 42;
    _ = this.Field_NoOverride;
    this.Field_NoOverride = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static_NoOverride;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_BaseInvoker.Target.Field_Static_NoOverride = 42;
  }
}