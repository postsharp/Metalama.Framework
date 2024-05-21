﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.AspectTesting.Licensing;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Testing.AspectTesting;

/// <summary>
/// Represents the properties read from assembly metadata and set from the MSBuild project.
/// </summary>
internal sealed record TestAssemblyMetadata(
    string ProjectDirectory,
    string SourceDirectory,
    ImmutableArray<string> ParserSymbols,
    string TargetFramework,
    bool MustLaunchDebugger,
    ImmutableArray<TestAssemblyReference> AssemblyReferences,

    // ReSharper disable once NotAccessedPositionalProperty.Global
    ImmutableArray<TestAssemblyReference> AnalyzerReferences,
    string? GlobalUsingsFile,
    TestFrameworkLicenseStatus License,
    ImmutableArray<string> IgnoredWarnings )
{
    public TestProjectReferences ToProjectReferences()
        => new(
            [..this.AssemblyReferences.Select( x => x.ToMetadataReference()! )],
            this.GlobalUsingsFile );
}