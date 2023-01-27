internal class TargetClass
{
  private global::System.Int32 _field;
  [global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedSource_BaseInvoker.TestAttribute]
  public global::System.Int32 Field
  {
    get
    {
      return this._field;
    }
    set
    {
      this._field = value;
    }
  }
}