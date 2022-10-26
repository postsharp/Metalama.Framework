[MyAspect]
public class C<T>
{
  public static void Method()
  {
  }
  public void Introduced()
  {
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.GenericClass.C<T>.Method();
    global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.GenericClass.C<global::System.Int32>.Method();
  }
}