// --- AspectInheritanceAllowed.cs ---
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceAllowed;
class Dummy
{
} // --- _AspectInheritance.cs ---
using  Metalama . Framework . Aspects ;  using  System ;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
class NonInstantiatedInheritableAspect : OverrideMethodAspect
{
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
class InstantiatedInheritableAspect : OverrideMethodAspect
{
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
class NonInstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
  public bool IsInheritable { get; init; }
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
class InstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
  public bool IsInheritable { get; init; }
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
class BaseClass
{
  [InstantiatedInheritableAspect]
  [InstatiatedConditionallyInheritableAspect(IsInheritable = true)]
  public virtual void TargetMethod()
  {
    global::System.Console.WriteLine("InstantiatedInheritableAspect");
    global::System.Console.WriteLine("InstatiatedConditionallyInheritableAspect");
    return;
  }
}
class InheritingClass : BaseClass
{
  public override void TargetMethod()
  {
    global::System.Console.WriteLine("InstantiatedInheritableAspect");
    global::System.Console.WriteLine("InstatiatedConditionallyInheritableAspect");
    return;
  }
}