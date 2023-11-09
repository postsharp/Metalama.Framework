using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
namespace Metalama.Framework.Tests.Integration.Tests.TestFramework.Introduction;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class C
{
  private void M()
  {
    this.IntroducedMethod();
  }
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  class Fabric : TypeFabric
  {
    [Introduce]
    public void IntroducedMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  public void IntroducedMethod()
  {
  }
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052