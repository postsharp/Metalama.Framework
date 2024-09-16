[TheAspect]
class C
{
  public class Builder
  {
    private global::System.Object Template()
    {
      return (global::System.Object)null !;
    }
  }
}
class D : C
{
  public new class Builder
  {
    private global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug35456.C.Builder Template()
    {
      return (global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug35456.C.Builder)null !;
    }
  }
}