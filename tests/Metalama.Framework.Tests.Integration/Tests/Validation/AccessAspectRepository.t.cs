using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;
namespace Metalama.Framework.Tests.Integration.Tests.Validation.AccessAspectRepository;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class TheAspect : TypeAspect
{
  public override void BuildAspect(IAspectBuilder<INamedType> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
  private void ValidateReference(in ReferenceValidationContext context) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
[TheAspect]
internal class C
{
  private C _f;
}