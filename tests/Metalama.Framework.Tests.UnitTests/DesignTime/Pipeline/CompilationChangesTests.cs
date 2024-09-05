// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using static Metalama.Testing.UnitTesting.TestCompilationFactory;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.Pipeline;

#pragma warning disable VSTHRD200

public sealed partial class CompilationChangesTests
{
    [Fact]
    public async Task AddPortableExecutableReference()
    {
        using var testContext = this.CreateTestContext();
        var code = new Dictionary<string, string> { { "code.cs", "" } };
        var compilation1 = CreateCSharpCompilation( code ).WithReferences( Enumerable.Empty<MetadataReference>() );
        var compilation2 = CreateCSharpCompilation( code );

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
        var masterCompilation1 = CreateCSharpCompilation( code, name: "Master" ).WithReferences( Enumerable.Empty<MetadataReference>() );

        var dependentCompilation1 = CreateCSharpCompilation(
            code,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation1.ToMetadataReference() } );

        var masterCompilation2 = CreateCSharpCompilation( code, name: "Master" );

        var dependentCompilation2 = CreateCSharpCompilation(
            code,
            name: "Dependent",
            additionalReferences: new[] { masterCompilation2.ToMetadataReference() } );

        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );
        var changes = await projectVersionProvider.GetCompilationChangesAsync( dependentCompilation1, dependentCompilation2 );
        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
    }

    [Fact]
    public async Task ReferencedCompilationModifiedNone()
    {
        using var testContext = this.CreateTestContext();
        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var emptyCode = new Dictionary<string, string>();

        var code1 = new Dictionary<string, string> { { "code.cs", "class C1;" } };

        var dependencyCompilation1 = CreateCSharpCompilation( code1, name: "Dependency" );
        var mainCompilation1 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation1.ToMetadataReference()] );

        var mainProjectVersion1 = await projectVersionProvider.GetCompilationVersionAsync( mainCompilation1 );

        var code2 = new Dictionary<string, string> { { "code.cs", "class C2;" } };

        var dependencyCompilation2 = CreateCSharpCompilation( code2, name: "Dependency" );
        var mainCompilation2 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation2.ToMetadataReference()] );

        var changes12 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation2 );
        Assert.Same( mainProjectVersion1, changes12.OldProjectVersionDangerous );
        Assert.Same( mainCompilation2, changes12.NewProjectVersion.Compilation );

        var dependencyCompilation3 = CreateCSharpCompilation( code2, name: "Dependency" );
        var mainCompilation3 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation3.ToMetadataReference()] );

        var changes13 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation3 );
        Assert.Same( mainProjectVersion1, changes13.OldProjectVersionDangerous );
        Assert.Same( mainCompilation3, changes13.NewProjectVersion.Compilation );

        var dependencyChange = Assert.Single( changes13.ReferencedCompilationChanges ).Value;

        Assert.Equal( ReferenceChangeKind.Modified, dependencyChange.ChangeKind );
        Assert.Same( dependencyCompilation1, dependencyChange.OldCompilationDangerous );
        Assert.Same( dependencyCompilation3, dependencyChange.NewCompilation );
    }

    [Fact]
    public async Task ReferencedCompilationNoneModified()
    {
        using var testContext = this.CreateTestContext();
        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var emptyCode = new Dictionary<string, string>();

        var code1 = new Dictionary<string, string> { { "code.cs", "class C1;" } };

        var dependencyCompilation1 = CreateCSharpCompilation( code1, name: "Dependency" );
        var mainCompilation1 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation1.ToMetadataReference()] );

        var mainProjectVersion1 = await projectVersionProvider.GetCompilationVersionAsync( mainCompilation1 );

        var dependencyCompilation2 = CreateCSharpCompilation( code1, name: "Dependency" );
        var mainCompilation2 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation2.ToMetadataReference()] );

        var changes12 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation2 );
        Assert.Same( mainProjectVersion1, changes12.OldProjectVersionDangerous );
        Assert.Same( mainCompilation2, changes12.NewProjectVersion.Compilation );

        var code3 = new Dictionary<string, string> { { "code.cs", "class C3;" } };

        var dependencyCompilation3 = CreateCSharpCompilation( code3, name: "Dependency" );
        var mainCompilation3 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation3.ToMetadataReference()] );

        var changes13 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation3 );
        Assert.Same( mainProjectVersion1, changes13.OldProjectVersionDangerous );
        Assert.Same( mainCompilation3, changes13.NewProjectVersion.Compilation );

        var dependencyChange = Assert.Single( changes13.ReferencedCompilationChanges ).Value;

        Assert.Equal( ReferenceChangeKind.Modified, dependencyChange.ChangeKind );
        Assert.Same( dependencyCompilation1, dependencyChange.OldCompilationDangerous );
        Assert.Same( dependencyCompilation3, dependencyChange.NewCompilation );
    }

    [Fact]
    public async Task ReferencedCompilationAddedRemoved()
    {
        using var testContext = this.CreateTestContext();
        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var mainCompilation1 = CreateEmptyCSharpCompilation( "Main" );

        var mainProjectVersion1 = await projectVersionProvider.GetCompilationVersionAsync( mainCompilation1 );

        var dependencyCompilation2 = CreateEmptyCSharpCompilation( "Dependency" );
        var mainCompilation2 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation2.ToMetadataReference()] );

        var changes12 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation2 );
        Assert.Same( mainProjectVersion1, changes12.OldProjectVersionDangerous );
        Assert.Same( mainCompilation2, changes12.NewProjectVersion.Compilation );

        var mainCompilation3 = CreateEmptyCSharpCompilation( "Main" );

        var changes13 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation3 );
        Assert.Same( mainProjectVersion1, changes13.OldProjectVersionDangerous );
        Assert.Same( mainCompilation3, changes13.NewProjectVersion.Compilation );

        Assert.Empty( changes13.ReferencedCompilationChanges );
    }

    [Fact]
    public async Task ReferencedCompilationAddedModified()
    {
        using var testContext = this.CreateTestContext();
        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var mainCompilation1 = CreateEmptyCSharpCompilation( "Main" );

        var mainProjectVersion1 = await projectVersionProvider.GetCompilationVersionAsync( mainCompilation1 );

        var code2 = new Dictionary<string, string> { { "code.cs", "class C2;" } };

        var dependencyCompilation2 = CreateCSharpCompilation( code2, name: "Dependency" );
        var mainCompilation2 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation2.ToMetadataReference()] );

        var changes12 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation2 );
        Assert.Same( mainProjectVersion1, changes12.OldProjectVersionDangerous );
        Assert.Same( mainCompilation2, changes12.NewProjectVersion.Compilation );

        var code3 = new Dictionary<string, string> { { "code.cs", "class C3;" } };

        var dependencyCompilation3 = CreateCSharpCompilation( code3, name: "Dependency" );
        var mainCompilation3 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation3.ToMetadataReference()] );

        var changes13 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation3 );
        Assert.Same( mainProjectVersion1, changes13.OldProjectVersionDangerous );
        Assert.Same( mainCompilation3, changes13.NewProjectVersion.Compilation );

        var dependencyChange = Assert.Single( changes13.ReferencedCompilationChanges ).Value;

        Assert.Equal( ReferenceChangeKind.Added, dependencyChange.ChangeKind );
        Assert.Null( dependencyChange.OldCompilationDangerous );
        Assert.Same( dependencyCompilation3, dependencyChange.NewCompilation );
    }

    [Fact]
    public async Task ReferencedCompilationModifiedModified()
    {
        using var testContext = this.CreateTestContext();
        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var emptyCode = new Dictionary<string, string>();

        var code1 = new Dictionary<string, string> { { "code.cs", "class C1;" } };

        var dependencyCompilation1 = CreateCSharpCompilation( code1, name: "Dependency" );
        var mainCompilation1 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation1.ToMetadataReference()] );

        var mainProjectVersion1 = await projectVersionProvider.GetCompilationVersionAsync( mainCompilation1 );

        var code2 = new Dictionary<string, string> { { "code.cs", "class C2;" } };

        var dependencyCompilation2 = CreateCSharpCompilation( code2, name: "Dependency" );
        var mainCompilation2 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation2.ToMetadataReference()] );

        var changes12 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation2 );
        Assert.Same( mainProjectVersion1, changes12.OldProjectVersionDangerous );
        Assert.Same( mainCompilation2, changes12.NewProjectVersion.Compilation );

        var code3 = new Dictionary<string, string> { { "code.cs", "class C3;" } };

        var dependencyCompilation3 = CreateCSharpCompilation( code3, name: "Dependency" );
        var mainCompilation3 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation3.ToMetadataReference()] );

        var changes13 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation3 );
        Assert.Same( mainProjectVersion1, changes13.OldProjectVersionDangerous );
        Assert.Same( mainCompilation3, changes13.NewProjectVersion.Compilation );

        var dependencyChange = Assert.Single( changes13.ReferencedCompilationChanges ).Value;

        Assert.Equal( ReferenceChangeKind.Modified, dependencyChange.ChangeKind );
        Assert.Same( dependencyCompilation1, dependencyChange.OldCompilationDangerous );
        Assert.Same( dependencyCompilation3, dependencyChange.NewCompilation );
    }

    [Fact]
    public async Task ReferencedCompilationModifiedRemoved()
    {
        using var testContext = this.CreateTestContext();
        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var emptyCode = new Dictionary<string, string>();

        var code1 = new Dictionary<string, string> { { "code.cs", "class C1;" } };

        var dependencyCompilation1 = CreateCSharpCompilation( code1, name: "Dependency" );
        var mainCompilation1 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation1.ToMetadataReference()] );

        var mainProjectVersion1 = await projectVersionProvider.GetCompilationVersionAsync( mainCompilation1 );

        var code2 = new Dictionary<string, string> { { "code.cs", "class C2;" } };

        var dependencyCompilation2 = CreateCSharpCompilation( code2, name: "Dependency" );
        var mainCompilation2 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation2.ToMetadataReference()] );

        var changes12 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation2 );
        Assert.Same( mainProjectVersion1, changes12.OldProjectVersionDangerous );
        Assert.Same( mainCompilation2, changes12.NewProjectVersion.Compilation );

        var mainCompilation3 = CreateEmptyCSharpCompilation( "Main" );

        var changes13 = await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation3 );
        Assert.Same( mainProjectVersion1, changes13.OldProjectVersionDangerous );
        Assert.Same( mainCompilation3, changes13.NewProjectVersion.Compilation );

        var dependencyChange = Assert.Single( changes13.ReferencedCompilationChanges ).Value;

        Assert.Equal( ReferenceChangeKind.Removed, dependencyChange.ChangeKind );
        Assert.Same( dependencyCompilation1, dependencyChange.OldCompilationDangerous );
        Assert.Null( dependencyChange.NewCompilation );
    }

    [Fact]
    public async Task IncorrectDependencyMerge()
    {
        // Regression test for issue #35359 in MergeReferencedProjectChangesAsync with ReferenceChangeKind.None.

        using var testContext = this.CreateTestContext();
        var projectVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var code = new Dictionary<string, string> { { "code.cs", "class C;" } };
        var dependencyCompilation1 = CreateCSharpCompilation( code, name: "Dependency" );
        var mainCompilation1 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation1.ToMetadataReference()] );

        var code3 = new Dictionary<string, string> { { "code.cs", "class C3;" } };
        var dependencyCompilation3 = CreateCSharpCompilation( code3, name: "Dependency" );
        var mainCompilation3 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation3.ToMetadataReference()] );

        var code4 = new Dictionary<string, string> { { "code.cs", "class C4;" } };
        var dependencyCompilation4 = CreateCSharpCompilation( code4, name: "Dependency" );
        var mainCompilation4 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation4.ToMetadataReference()] );

        async Task Impl()
        {
            var dependencyCompilation2 = CreateCSharpCompilation( code, name: "Dependency" );
            var mainCompilation2 = CreateEmptyCSharpCompilation( "Main", [dependencyCompilation2.ToMetadataReference()] );

            await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation2 );

            await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation3 );
        }

        await Impl();

        GC.Collect();

        await projectVersionProvider.GetCompilationChangesAsync( mainCompilation1, mainCompilation4 );
    }
}