using System;
namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssemblyChildAspect
{
  public class C : I
  {
    public void M()
    {
      global::System.Console.WriteLine("From ChildAspect");
      Console.WriteLine("Hello, world.");
      return;
    }
  }
}