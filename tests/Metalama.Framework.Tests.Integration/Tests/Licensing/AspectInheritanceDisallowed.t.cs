// --- AspectInheritanceDisallowed.cs ---
// Warning LAMA0802 on ``: `The 'Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance.InstantiatedInheritableAspect', 'Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance.InstatiatedConditionallyInheritableAspect' aspects shall not be automatically inherited because this feature is not covered by your license. Aspects must be explicitly added to all derived types using custom attributes.`
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceDisallowed;
class Dummy
{
} // --- _AspectInheritance.cs ---
// Warning LAMA0802 on ``: `The 'Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance.InstantiatedInheritableAspect', 'Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance.InstatiatedConditionallyInheritableAspect' aspects shall not be automatically inherited because this feature is not covered by your license. Aspects must be explicitly added to all derived types using custom attributes.`
using  Metalama . Framework . Aspects ;  using  Metalama . Framework . Code ;  using  System ;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritance;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
internal class NonInstantiatedInheritableAspect : OverrideMethodAspect
{
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
internal class InstantiatedInheritableAspect : OverrideMethodAspect
{
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
internal class NonInstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
  public bool IsInheritable { get; init; }
  bool IConditionallyInheritableAspect.IsInheritable(IDeclaration targetDeclaration, IAspectInstance aspectInstance) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[Inheritable]
internal class InstatiatedConditionallyInheritableAspect : OverrideMethodAspect, IConditionallyInheritableAspect
{
  public bool IsInheritable { get; init; }
  bool IConditionallyInheritableAspect.IsInheritable(IDeclaration targetDeclaration, IAspectInstance aspectInstance) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
internal class BaseClass
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
internal class InheritingClass : BaseClass
{
  public override void TargetMethod()
  {
  }
}