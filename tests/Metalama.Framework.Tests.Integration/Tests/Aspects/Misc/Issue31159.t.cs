using System;
using Metalama.Framework.Aspects;
namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue31159;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[RunTimeOrCompileTime]
public class DerivedAspect : BaseAspect
{
    public override void Validate(dynamic? value) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
// Target.
public interface I
{
    void M( [DerivedAspect] int x );
}
public class C : I
{
  public void M([DerivedAspect] int x)
  {
    global::System.Console.WriteLine("Again");
    }
}
