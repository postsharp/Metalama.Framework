using System.Collections.Immutable;
using Metalama.Framework.Serialization;
namespace Metalama.Framework.Tests.AspectTests.Tests.Aspects.Bugs.SerializableNullableImmutableArray;
#pragma warning disable CS0169
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
internal class C : ICompileTimeSerializable
{
  private ImmutableArray<string>? f;
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052