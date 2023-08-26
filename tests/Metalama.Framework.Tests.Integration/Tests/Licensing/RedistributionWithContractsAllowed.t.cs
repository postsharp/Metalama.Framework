// --- RedistributionWithContractsAllowed.cs ---
using Metalama.Framework.Aspects;
using System;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionWithContractsAllowed;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class NonRedistributionAspect1 : OverrideMethodAspect
{
  public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class Contract1 : ContractAspect
{
  public override void Validate(dynamic? value) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
class TargetClass
{
  [NonRedistributionAspect1]
  public void TargetMethod([Contract1] int? targetParameter)
  {
    if (targetParameter == null)
    {
      throw new global::System.ArgumentNullException(nameof(targetParameter), $"Validated by {("Contract1")}.");
    }
    global::System.Console.WriteLine("TargetClass.TargetMethod(int?) enhanced by NonRedistributionAspect1");
  }
} // --- _RedistributionWithContracts.cs ---
using  Metalama . Framework . Tests . Integration . Tests . Licensing . RedistributionWithContracts . Dependency ;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.RedistributionWithContracts;
class RedistributionTargetClass
{
  [RedistributionAspect1]
  [RedistributionAspect2]
  void RedistributionTargetMethod([RedistributionContract1][RedistributionContract2] int? targetParameter)
  {
    global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod(int?) enhanced by RedistributionAspect1");
    global::System.Console.WriteLine("RedistributionTargetClass.RedistributionTargetMethod(int?) enhanced by RedistributionAspect2");
    if (targetParameter == null)
    {
      throw new global::System.ArgumentNullException(nameof(targetParameter), $"Validated by {("RedistributionContract1")}.");
    }
    if (targetParameter == null)
    {
      throw new global::System.ArgumentNullException(nameof(targetParameter), $"Validated by {("RedistributionContract2")}.");
    }
    return;
  }
}