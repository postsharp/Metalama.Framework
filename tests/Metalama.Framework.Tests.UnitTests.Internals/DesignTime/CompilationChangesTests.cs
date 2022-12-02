// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200

public partial class CompilationChangesTests
{
    [Fact]
    public async Task AddPortableExecutableReference()
    {
        using var testContext = this.CreateTestContext();
        var code = new Dictionary<string, string> { { "code.cs", "" } };
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code ).WithReferences( Enumerable.Empty<MetadataReference>() );
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code );

        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );
        var changes = await projectVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
    }

    [Fact]
    public async Task AddPortableExecutableReferenceInDependency()
    {
        using var testContext = this.CreateTestContext();
        var code = new Dictionary<string, string> { { "code.cs", "" } };
        var masterCompilation1 = TestCompilationFactory.CreateCSharpCompilation( code, name: "Master" ).WithReferences( Enumerable.Empty<MetadataReference>() );

        var dependentCompilation1 = TestCompilationFactory.CreateCSharpCompilation(
            code,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation1.ToMetadataReference() } );

        var masterCompilation2 = TestCompilationFactory.CreateCSharpCompilation( code, name: "Master" );

        var dependentCompilation2 = TestCompilationFactory.CreateCSharpCompilation(
            code,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation2.ToMetadataReference() } );

        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );
        var changes = await projectVersionProvider.GetCompilationChangesAsync( dependentCompilation1, dependentCompilation2 );
        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
    }
}