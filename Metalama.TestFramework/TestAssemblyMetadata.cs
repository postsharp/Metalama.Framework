// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using System.Linq;

namespace Metalama.TestFramework;

/// <summary>
/// Represents the properties read from assembly metadata and set from the MSBuild project.
/// </summary>
internal record TestAssemblyMetadata(
    bool MustLaunchDebugger,
    ImmutableArray<TestAssemblyReference> AssemblyReferences,
    string? GlobalUsingsFile )
{
    public TestProjectReferences ToProjectReferences()
        => new( this.AssemblyReferences.Select( x => x.ToMetadataReference()! ).ToImmutableArray(), this.GlobalUsingsFile );
}