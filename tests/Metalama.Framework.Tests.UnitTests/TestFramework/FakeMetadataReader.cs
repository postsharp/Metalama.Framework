// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Testing.AspectTesting;
using Metalama.Testing.AspectTesting.Licensing;
using System.Collections.Immutable;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.TestFramework;

internal sealed class FakeMetadataReader : ITestAssemblyMetadataReader
{
    private readonly string _projectDirectory;

    public FakeMetadataReader( string projectDirectory )
    {
        this._projectDirectory = projectDirectory;
    }

    public TestAssemblyMetadata GetMetadata( IAssemblyInfo assembly )
        => new(
            this._projectDirectory,
            ImmutableArray<string>.Empty,
            "net6.0",
            false,
            ImmutableArray<TestAssemblyReference>.Empty,
            null,
            new TestFrameworkLicenseStatus( "test", null, false ),
            ImmutableArray<string>.Empty );
}