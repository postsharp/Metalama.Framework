[IntroductionAttribute]
public abstract class TargetType
{
  public abstract int Property { get; set; }
  public abstract void Method();
  class TestNestedType : global::Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_Abstract.TargetType
  {
    private global::System.Int32 _property;
    public override global::System.Int32 Property
    {
      get
      {
        return this._property;
      }
      set
      {
        this._property = value;
      }
    }
    public override void Method()
    {
    }
  }
}