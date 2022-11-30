// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Testing.UnitTesting.Options;

/// <summary>
/// Options that influence the <see cref="UnitTestSuite.CreateTestContext(Metalama.Testing.UnitTesting.Options.TestContextOptions?, Metalama.Framework.Engine.Services.IAdditionalServiceCollection?)"/>
/// method.
/// </summary>
public sealed class TestContextOptions
{
    /// <summary>
    /// Gets the default <see cref="TestContextOptions"/> value.
    /// </summary>
    public static TestContextOptions Default { get; } = new();

    /// <summary>
    /// Gets the set of MSBuild properties exposed to the tests.
    /// </summary>
    public ImmutableDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// Gets the set of compiler plug-ins exposed to the tests.
    /// </summary>
    internal ImmutableArray<object> PlugIns { get; init; } = ImmutableArray<object>.Empty;

    /// <summary>
    /// Gets a value indicating whether the output code should be formatted.
    /// </summary>
    public bool FormatOutput { get; init; }

    /// <summary>
    /// Gets a value indicating whether the compile-time code should be formatted.
    /// </summary>
    public bool FormatCompileTimeCode { get; init; }

    /// <summary>
    /// Gets the set of assemblies that should be added as references to the compile-time compilaiton.
    /// </summary>
    public ImmutableArray<Assembly> AdditionalAssemblies { get; init; } = ImmutableArray<Assembly>.Empty;

    /// <summary>
    /// Gets a value indicating whether an error should be reported if all aspect classes
    /// are not strongly ordered.
    /// </summary>
    public bool RequireOrderedAspects { get; init; }

    internal bool HasSourceGeneratorTouchFile { get; init; }

    internal bool HasBuildTouchFile { get; init; }
}