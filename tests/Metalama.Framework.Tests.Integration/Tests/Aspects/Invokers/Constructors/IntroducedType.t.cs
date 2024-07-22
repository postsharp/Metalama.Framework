public class TargetClass
{
  [InvokerAspect]
  public void Invoker()
  { // Invoke new <introduced>();
    new global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Constructors.IntroducedType.TargetClass.IntroducedType();
    return;
  }
  class IntroducedType
  {
    public IntroducedType()
    {
    }
  }
}