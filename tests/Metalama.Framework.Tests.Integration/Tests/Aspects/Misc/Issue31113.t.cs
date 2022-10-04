using System.Linq;
using Metalama.Framework.Aspects;
namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue31113;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class MyAspect : TypeAspect
{
  [Introduce]
  public void Method() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[MyAspect]
class C
{
  void M()
  {
  }
  public void Method()
  {
    this.M();
  }
}