// Final Compilation.Emit failed.
// Error CS0246 on `Foo`: `The type or namespace name 'Foo' could not be found (are you missing a using directive or an assembly reference?)`
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
// Test that using a type that does not exist produces only C# errors, and not confusing Metalama errors.
namespace Metalama.Framework.Tests.Integration.Aspects.InvalidCode.MissingType;
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class C
{
    [CompileTime]
    void M(IAspectBuilder<Foo> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
