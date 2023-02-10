[Before]
[Test]
[After]
public class Target
{
  public int Field;
  public static int Field_Static;
  public global::System.Int32 IntroducedField;
  public global::System.Int32 IntroducedField_Static;
  public void Introduced()
  {
    _ = this.Field;
    this.Field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.Field_FinalInvoker.Target.Field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.Field_FinalInvoker.Target.Field_Static = 42;
  }
  public void IntroducedAfter()
  {
    _ = this.Field;
    this.Field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.Field_FinalInvoker.Target.Field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.Field_FinalInvoker.Target.Field_Static = 42;
    _ = this.IntroducedField;
    this.IntroducedField = 42;
    _ = this.IntroducedField_Static;
    this.IntroducedField_Static = 42;
  }
  public void IntroducedBefore()
  {
    _ = this.Field;
    this.Field = 42;
    _ = global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.Field_FinalInvoker.Target.Field_Static;
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.Field_FinalInvoker.Target.Field_Static = 42;
  }
}