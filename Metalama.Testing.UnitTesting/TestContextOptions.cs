// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Testing.UnitTesting;

/// <summary>
/// Options that influence the <see cref="UnitTestClass.CreateTestContext(Metalama.Testing.UnitTesting.TestContextOptions?,Metalama.Framework.Engine.Services.IAdditionalServiceCollection?)"/>
/// method.
/// </summary>
[PublicAPI]
public sealed record TestContextOptions
{
    public bool RequiresExclusivity { get; init; }

    /// <summary>
    /// Gets the default <see cref="TestContextOptions"/> value.
    /// </summary>
    public static TestContextOptions Default { get; } = new();

    /// <summary>
    /// Gets the set of MSBuild properties exposed to the tests.
    /// </summary>
    public ImmutableDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;

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

    internal bool RoslynIsCompileTimeOnly { get; init; } = true;

    /// <summary>
    /// Gets the list of references that will be added to compilations created in this context.
    /// </summary>
    public ImmutableArray<PortableExecutableReference> References { get; init; } =
        TestCompilationFactory.GetMetadataReferences().ToImmutableArray();

    /// <summary>
    /// Gets the test timeout period, after which the <see cref="TestContext.CancellationToken"/> of the <see cref="TestContext"/> is signalled.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds( 30 );

    public string? ProjectName { get; init; }
}