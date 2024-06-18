using System.Collections.Immutable;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.SerializableNullableImmutableArray;

#pragma warning disable CS0169

internal class C : ICompileTimeSerializable
{
    private ImmutableArray<string>? f;
}