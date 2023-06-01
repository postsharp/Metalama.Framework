﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.Pipeline;

#pragma warning disable VSTHRD200 // Async method names must have "Async" suffix.

public sealed class ProjectVersionProviderTests : DesignTimeTestBase
{
    [Fact]
    public async Task DifferentCompilationWithNoChangeAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code );
        var compilationChanges1 = await compilationVersionProvider.GetCompilationChangesAsync( null, compilation1 );
        Assert.Same( compilation1, compilationChanges1.NewProjectVersion.CompilationToAnalyze );
        Assert.False( compilationChanges1.IsIncremental );
        Assert.True( compilationChanges1.HasChange );
        Assert.True( compilationChanges1.HasCompileTimeCodeChange );
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 0, observer.ComputeIncrementalChangesEventCount );

        // Create a second compilation. We explicitly copy the references from the first compilation because references
        // may not be equal due to a change in the loaded assemblies in the AppDomain during the execution of the test.
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code ).WithReferences( compilation1.References );
        var compilationChanges2 = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.True( compilationChanges2.IsIncremental );
        Assert.False( compilationChanges2.HasChange );
        Assert.False( compilationChanges2.HasCompileTimeCodeChange );
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 1, observer.ComputeIncrementalChangesEventCount );

        // Calling GetCompilationVersion a second time with the same parameter should not trigger an computation of changes.
        _ = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 1, observer.ComputeIncrementalChangesEventCount );

        // Create a third compilation with no change.
        var compilation3 = TestCompilationFactory.CreateCSharpCompilation( code ).WithReferences( compilation1.References );
        var compilationChanges3 = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation3 );
        Assert.True( compilationChanges3.IsIncremental );
        Assert.False( compilationChanges3.HasChange );
        Assert.False( compilationChanges3.HasCompileTimeCodeChange );
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 2, observer.ComputeIncrementalChangesEventCount );
    }

    [Fact]
    public async Task SameCompilationWithoutPredecessorAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code );
        var changes1 = await compilationVersionProvider.GetCompilationChangesAsync( null, compilation1 );
        var changes2 = await compilationVersionProvider.GetCompilationChangesAsync( null, compilation1 );

        Assert.Null( changes1.OldCompilationVersion );
        Assert.Same( compilation1, changes1.NewProjectVersion.Compilation );
        Assert.False( changes1.IsIncremental );
        Assert.Same( changes1, changes2 );
    }

    [Fact]
    public async Task SameCompilationWithDifferentPredecessorAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code, name: "test" );
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code, name: "test" );
        var compilation3 = TestCompilationFactory.CreateCSharpCompilation( code, name: "test" );
        var changes1 = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation3 );
        var changes2 = await compilationVersionProvider.GetCompilationChangesAsync( compilation2, compilation3 );

        Assert.Same( compilation1, changes1.OldCompilationVersion!.Compilation );
        Assert.Same( compilation3, changes1.NewProjectVersion.Compilation );
        Assert.Same( compilation3, changes1.NewProjectVersion.CompilationToAnalyze );
        Assert.Same( compilation2, changes2.OldCompilationVersion!.Compilation );
        Assert.Same( compilation3, changes2.NewProjectVersion.Compilation );
    }

    [Fact]
    public async Task AddCompilationReference()
    {
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( dependentCode );

        var masterCode = new Dictionary<string, string> { { "code.cs", "class D{}" } };
        var masterCompilation = TestCompilationFactory.CreateCSharpCompilation( masterCode );

        var compilation2 = TestCompilationFactory.CreateCSharpCompilation(
            dependentCode,
            additionalReferences: new[] { masterCompilation.ToMetadataReference() } );

        var changes = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.Empty( changes.SyntaxTreeChanges );
        Assert.Single( changes.ReferencedCompilationChanges );
        var referencedCompilationChange = changes.ReferencedCompilationChanges.Single().Value;
        Assert.Equal( ReferenceChangeKind.Added, referencedCompilationChange.ChangeKind );
        Assert.Null( referencedCompilationChange.OldCompilation );
        Assert.Same( masterCompilation, referencedCompilationChange.NewCompilation );
        Assert.Null( referencedCompilationChange.Changes );
        Assert.True( referencedCompilationChange.HasCompileTimeCodeChange );
    }

    [Fact]
    public async Task RemoveCompilationReference()
    {
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var masterCode = new Dictionary<string, string> { { "code.cs", "class D{}" } };
        var masterCompilation = TestCompilationFactory.CreateCSharpCompilation( masterCode );

        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };

        var compilation1 = TestCompilationFactory.CreateCSharpCompilation(
            dependentCode,
            additionalReferences: new[] { masterCompilation.ToMetadataReference() } );

        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( dependentCode );

        var changes = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.Empty( changes.SyntaxTreeChanges );
        Assert.Single( changes.ReferencedCompilationChanges );
        var referencedCompilationChange = changes.ReferencedCompilationChanges.Single().Value;
        Assert.Equal( ReferenceChangeKind.Removed, referencedCompilationChange.ChangeKind );
        Assert.Null( referencedCompilationChange.NewCompilation );
        Assert.Same( masterCompilation, referencedCompilationChange.OldCompilation );
        Assert.Null( referencedCompilationChange.Changes );
        Assert.True( referencedCompilationChange.HasCompileTimeCodeChange );
    }

    [Fact]
    public async Task AddCompilationReferenceInCompilationReference()
    {
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var level1Code = new Dictionary<string, string> { { "code.cs", "class E {}" } };
        var compilationLevel1 = TestCompilationFactory.CreateCSharpCompilation( level1Code, name: "Level1" );

        var level2Code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilationLevel2WithoutLevel1Reference = TestCompilationFactory.CreateCSharpCompilation( level2Code, name: "Level2" );

        var compilationLevel2WithLevel1Reference = TestCompilationFactory.CreateCSharpCompilation(
            level2Code,
            name: "Level2",
            additionalReferences: new[] { compilationLevel1.ToMetadataReference() } );

        var level3Code = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };

        var compilationLevel3WithoutLevel1Reference = TestCompilationFactory.CreateCSharpCompilation(
            level3Code,
            name: "Level3",
            additionalReferences: new[] { compilationLevel2WithoutLevel1Reference.ToMetadataReference() } );

        var compilationLevel3WithLevel1Reference = TestCompilationFactory.CreateCSharpCompilation(
            level3Code,
            name: "Level3",
            additionalReferences: new[] { compilationLevel2WithLevel1Reference.ToMetadataReference() } );

        var changes = await compilationVersionProvider.GetCompilationChangesAsync(
            compilationLevel3WithoutLevel1Reference,
            compilationLevel3WithLevel1Reference );

        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.Empty( changes.SyntaxTreeChanges );
        Assert.Single( changes.ReferencedCompilationChanges );
        var level3ReferencedCompilationChange = changes.ReferencedCompilationChanges.Single().Value;
        Assert.Equal( ReferenceChangeKind.Modified, level3ReferencedCompilationChange.ChangeKind );
        Assert.Same( compilationLevel2WithoutLevel1Reference, level3ReferencedCompilationChange.OldCompilation );
        Assert.Same( compilationLevel2WithLevel1Reference, level3ReferencedCompilationChange.NewCompilation );
        Assert.True( level3ReferencedCompilationChange.HasCompileTimeCodeChange );
        Assert.NotNull( level3ReferencedCompilationChange.Changes );
        Assert.Empty( level3ReferencedCompilationChange.Changes!.SyntaxTreeChanges );
        Assert.Single( level3ReferencedCompilationChange.Changes!.ReferencedCompilationChanges );
        var level2ReferencedCompilationChange = level3ReferencedCompilationChange.Changes.ReferencedCompilationChanges.Single().Value;
        Assert.Equal( ReferenceChangeKind.Added, level2ReferencedCompilationChange.ChangeKind );
        Assert.Same( compilationLevel1, level2ReferencedCompilationChange.NewCompilation );
    }

    [Fact]
    public async Task IntermediateCompilationCanBeCollected()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        using var testContext = this.CreateTestContext();

        // Create the first compilation.
        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code );
        var compilationChanges1 = await compilationVersionProvider.GetCompilationChangesAsync( null, compilation1 );
        Assert.Same( compilation1, compilationChanges1.NewProjectVersion.CompilationToAnalyze );
        Assert.False( compilationChanges1.IsIncremental );
        Assert.True( compilationChanges1.HasChange );
        Assert.True( compilationChanges1.HasCompileTimeCodeChange );

        // Create a second compilation. We only keep a weak reference to it.
        var wr = await CreateIncrementalCompilation( code, compilation1.References, compilationVersionProvider, compilation1 );

        await Task.Yield();

        GC.Collect();
        Assert.False( wr.IsAlive );
    }

    private static async Task<WeakReference> CreateIncrementalCompilation(
        Dictionary<string, string> code,
        IEnumerable<MetadataReference> references,
        ProjectVersionProvider projectVersionProvider,
        Compilation compilation1 )
    {
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code ).WithReferences( references );
        var compilationChanges2 = await projectVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.True( compilationChanges2.IsIncremental );
        Assert.False( compilationChanges2.HasChange );
        Assert.False( compilationChanges2.HasCompileTimeCodeChange );

        return new WeakReference( compilation2 );
    }
}