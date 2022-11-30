// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Testing.Api.Options;

public sealed class TestContextOptions
{
    public static TestContextOptions Default { get; } = new();

    public ImmutableDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;

    internal ImmutableArray<object> PlugIns { get; init; } = ImmutableArray<object>.Empty;

    public bool FormatOutput { get; init; }

    public bool FormatCompileTimeCode { get; init; }

    public ImmutableArray<Assembly> AdditionalAssemblies { get; init; } = ImmutableArray<Assembly>.Empty;

    public bool RequireOrderedAspects { get; init; }

    internal bool HasSourceGeneratorTouchFile { get; init; }

    internal bool HasBuildTouchFile { get; init; }
}