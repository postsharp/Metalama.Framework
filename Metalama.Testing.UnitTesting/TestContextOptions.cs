// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Testing.UnitTesting;

/// <summary>
/// Options that influence the <see cref="UnitTestClass.CreateTestContext(Metalama.Testing.UnitTesting.TestContextOptions?,Metalama.Framework.Engine.Services.IAdditionalServiceCollection?)"/>
/// method.
/// </summary>
public sealed record TestContextOptions
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
    /// Gets the set of assemblies that should be added as references to the compile-time compilation.
    /// </summary>
    public ImmutableArray<Assembly> AdditionalAssemblies { get; init; } = ImmutableArray<Assembly>.Empty;

    /// <summary>
    /// Gets a value indicating whether an error should be reported if all aspect classes
    /// are not strongly ordered.
    /// </summary>
    public bool RequireOrderedAspects { get; init; }

    internal bool HasSourceGeneratorTouchFile { get; init; }

    internal bool HasBuildTouchFile { get; init; }

    /// <summary>
    /// Gets the list of references that will be added to compilations created in this context.
    /// </summary>
    public ImmutableArray<MetadataReference> References { get; init; } = TestCompilationFactory.GetMetadataReferences().ToImmutableArray<MetadataReference>();

    /// <summary>
    /// Gets the test timeout period, after which the <see cref="TestContext.CancellationToken"/> of the <see cref="TestContext"/> is signalled.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds( 30 );
}