// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class CompilationChangesProviderTests : DesignTimeTestBase
{
    [Fact]
    public async Task DifferentCompilationWithNoChangeAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var compilationVersionProvider = new CompilationVersionProvider( testContext.ServiceProvider.WithService( observer ) );
        var compilation1 = CreateCSharpCompilation( code );
        var compilationChanges1 = await compilationVersionProvider.GetCompilationChangesAsync( null, compilation1 );
        Assert.Same( compilation1, compilationChanges1.NewCompilationVersion.CompilationToAnalyze );
        Assert.False( compilationChanges1.IsIncremental );
        Assert.True( compilationChanges1.HasChange );
        Assert.True( compilationChanges1.HasCompileTimeCodeChange );
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 0, observer.ComputeIncrementalChangesEventCount );

        // Create a second compilation. We explicitly copy the references from the first compilation because references
        // may not be equal due to a change in the loaded assemblies in the AppDomain during the execution of the test.
        var compilation2 = CreateCSharpCompilation( code ).WithReferences( compilation1.References );
        var compilationChanges2 = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.True( compilationChanges2.IsIncremental );
        Assert.False( compilationChanges2.HasChange );
        Assert.False( compilationChanges2.HasCompileTimeCodeChange );
        Assert.Same( compilation1, compilationChanges2.NewCompilationVersion.CompilationToAnalyze ); // There is no change, so compilation1 is expected.
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 1, observer.ComputeIncrementalChangesEventCount );

        // Calling GetCompilationVersion a second time with the same parameter should not trigger an computation of changes.
        _ = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 1, observer.ComputeIncrementalChangesEventCount );

        // Create a third compilation with no change.
        var compilation3 = CreateCSharpCompilation( code ).WithReferences( compilation1.References );
        var compilationChanges3 = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation3 );
        Assert.True( compilationChanges3.IsIncremental );
        Assert.False( compilationChanges3.HasChange );
        Assert.False( compilationChanges3.HasCompileTimeCodeChange );
        Assert.Same( compilation1, compilationChanges3.NewCompilationVersion.CompilationToAnalyze ); // There is no change, so compilation1 is expected.
        Assert.Equal( 1, observer.ComputeNonIncrementalChangesEventCount );
        Assert.Equal( 2, observer.ComputeIncrementalChangesEventCount );
    }

    [Fact]
    public async Task SameCompilationWithoutPredecessorAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var compilationVersionProvider = new CompilationVersionProvider( testContext.ServiceProvider.WithService( observer ) );
        var compilation1 = CreateCSharpCompilation( code );
        _ = await compilationVersionProvider.GetCompilationChangesAsync( null, compilation1 );
        _ = await compilationVersionProvider.GetCompilationChangesAsync( null, compilation1 );
    }

    [Fact]
    public async Task SameCompilationWithDifferentPredecessorAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var compilationVersionProvider = new CompilationVersionProvider( testContext.ServiceProvider.WithService( observer ) );
        var compilation1 = CreateCSharpCompilation( code );
        var compilation2 = CreateCSharpCompilation( code );
        var compilation3 = CreateCSharpCompilation( code );
        var compilationVersion3a = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation3 );
        var compilationVersion3b = await compilationVersionProvider.GetCompilationChangesAsync( compilation2, compilation3 );
    }

    [Fact]
    public async Task AddCompilationReference()
    {
        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var compilationVersionProvider = new CompilationVersionProvider( testContext.ServiceProvider.WithService( observer ) );

        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };
        var compilation1 = CreateCSharpCompilation( dependentCode );

        var masterCode = new Dictionary<string, string> { { "code.cs", "class D{}" } };
        var masterCompilation = CreateCSharpCompilation( masterCode );

        var compilation2 = CreateCSharpCompilation( dependentCode, additionalReferences: new[] { masterCompilation.ToMetadataReference() } );

        var changes = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.Empty( changes.SyntaxTreeChanges );
        Assert.Single( changes.ReferencedCompilationChanges );
        var referencedCompilationChange = changes.ReferencedCompilationChanges.Single().Value;
        Assert.Equal( ReferencedCompilationChangeKind.Added, referencedCompilationChange.ChangeKind );
        Assert.Null( referencedCompilationChange.OldCompilation );
        Assert.Same( masterCompilation, referencedCompilationChange.NewCompilation );
        Assert.Null( referencedCompilationChange.Changes );
        Assert.True( referencedCompilationChange.HasCompileTimeCodeChange );
    }

    [Fact]
    public async Task RemoveCompilationReference()
    {
        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var compilationVersionProvider = new CompilationVersionProvider( testContext.ServiceProvider.WithService( observer ) );

        var masterCode = new Dictionary<string, string> { { "code.cs", "class D{}" } };
        var masterCompilation = CreateCSharpCompilation( masterCode );

        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };
        var compilation1 = CreateCSharpCompilation( dependentCode, additionalReferences: new[] { masterCompilation.ToMetadataReference() } );

        var compilation2 = CreateCSharpCompilation( dependentCode );

        var changes = await compilationVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.Empty( changes.SyntaxTreeChanges );
        Assert.Single( changes.ReferencedCompilationChanges );
        var referencedCompilationChange = changes.ReferencedCompilationChanges.Single().Value;
        Assert.Equal( ReferencedCompilationChangeKind.Removed, referencedCompilationChange.ChangeKind );
        Assert.Null( referencedCompilationChange.NewCompilation );
        Assert.Same( masterCompilation, referencedCompilationChange.OldCompilation );
        Assert.Null( referencedCompilationChange.Changes );
        Assert.True( referencedCompilationChange.HasCompileTimeCodeChange );
    }

    [Fact]
    public async Task AddCompilationReferenceInCompilationReference()
    {
        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var compilationVersionProvider = new CompilationVersionProvider( testContext.ServiceProvider.WithService( observer ) );

        var level1Code = new Dictionary<string, string> { { "code.cs", "class E {}" } };
        var compilationLevel1 = CreateCSharpCompilation( level1Code, name: "Level1" );

        var level2Code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilationLevel2WithoutLevel1Reference = CreateCSharpCompilation( level2Code, name: "Level2" );

        var compilationLevel2WithLevel1Reference = CreateCSharpCompilation(
            level2Code,
            name: "Level2",
            additionalReferences: new[] { compilationLevel1.ToMetadataReference() } );

        var level3Code = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };

        var compilationLevel3WithoutLevel1Reference = CreateCSharpCompilation(
            level3Code,
            name: "Level3",
            additionalReferences: new[] { compilationLevel2WithoutLevel1Reference.ToMetadataReference() } );

        var compilationLevel3WithLevel1Reference = CreateCSharpCompilation(
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
        Assert.Equal( ReferencedCompilationChangeKind.Modified, level3ReferencedCompilationChange.ChangeKind );
        Assert.Same( compilationLevel2WithoutLevel1Reference, level3ReferencedCompilationChange.OldCompilation );
        Assert.Same( compilationLevel2WithLevel1Reference, level3ReferencedCompilationChange.NewCompilation );
        Assert.True( level3ReferencedCompilationChange.HasCompileTimeCodeChange );
        Assert.NotNull( level3ReferencedCompilationChange.Changes );
        Assert.Empty( level3ReferencedCompilationChange.Changes!.SyntaxTreeChanges );
        Assert.Single( level3ReferencedCompilationChange.Changes!.ReferencedCompilationChanges );
        var level2ReferencedCompilationChange = level3ReferencedCompilationChange.Changes.ReferencedCompilationChanges.Single().Value;
        Assert.Equal( ReferencedCompilationChangeKind.Added, level2ReferencedCompilationChange.ChangeKind );
        Assert.Same( compilationLevel1, level2ReferencedCompilationChange.NewCompilation );
    }
}