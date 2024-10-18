[Introduction]
public class TargetClass<T> : global::Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.Generic_Closed_TypeParameter.IInterface<T>, global::Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.Generic_Closed_TypeParameter.IInterface<T[]>, global::Metalama.Framework.Tests.AspectTests.TestInputs.Aspects.Introductions.Interfaces.Generic_Closed_TypeParameter.IInterface<global::System.Tuple<T, T[]>>
{
  public void Foo(T t)
  {
  }
  public void Foo(T[] t)
  {
  }
  public void Foo(global::System.Tuple<T, T[]> t)
  {
  }
}