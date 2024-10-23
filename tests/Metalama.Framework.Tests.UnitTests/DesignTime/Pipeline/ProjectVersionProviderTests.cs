// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code, name: nameof( this.DifferentCompilationWithNoChangeAsync ) );
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
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code, name: nameof( this.DifferentCompilationWithNoChangeAsync ) )
            .WithReferences( compilation1.References );

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
        var compilation3 = TestCompilationFactory.CreateCSharpCompilation( code, name: nameof( this.DifferentCompilationWithNoChangeAsync ) )
            .WithReferences( compilation1.References );

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

        Assert.Null( changes1.OldProjectVersionDangerous );
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

        Assert.Same( compilation1, changes1.OldProjectVersionDangerous!.Compilation );
        Assert.Same( compilation3, changes1.NewProjectVersion.Compilation );
        Assert.Same( compilation3, changes1.NewProjectVersion.CompilationToAnalyze );
        Assert.Same( compilation2, changes2.OldProjectVersionDangerous!.Compilation );
        Assert.Same( compilation3, changes2.NewProjectVersion.Compilation );
    }

    [Fact]
    public async Task AddCompilationReference()
    {
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var dependentCode =
            new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects;  class C {}" } };

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
        Assert.Null( referencedCompilationChange.OldCompilationDangerous );
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

        var dependentCode =
            new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects;  class C {}" } };

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
        Assert.Same( masterCompilation, referencedCompilationChange.OldCompilationDangerous );
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

        var level3Code = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects;  class C {}" } };

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
        Assert.Same( compilationLevel2WithoutLevel1Reference, level3ReferencedCompilationChange.OldCompilationDangerous );
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
        var compilation1 = TestCompilationFactory.CreateCSharpCompilation( code, name: nameof( this.IntermediateCompilationCanBeCollected ) );
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

    [Fact]
    public async Task DependencyChange()
    {
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var dependencyCode1 =
            new Dictionary<string, string> { { "code1.cs", "using Metalama.Framework.Aspects; public class C {}" } };

        var dependencyCompilation1 = TestCompilationFactory.CreateCSharpCompilation( dependencyCode1 );

        var dependentCode = new Dictionary<string, string> { { "code2.cs", "public class D : C {}" } };
        var dependentCompilation1 =
            TestCompilationFactory.CreateCSharpCompilation(
                dependentCode,
                additionalReferences: new[] { dependencyCompilation1.ToMetadataReference() } );

        var dependencyCode2 =
            new Dictionary<string, string> { { "code1.cs", "using Metalama.Framework.Aspects; public class C { int F; }" } };

        var dependencyCompilation2 = TestCompilationFactory.ReplaceSyntaxTrees( dependencyCompilation1, dependencyCode2 );
        var dependentCompilation2 =
            TestCompilationFactory.ReplaceReferences(
                dependentCompilation1,
                new[] { dependencyCompilation2.ToMetadataReference() } );

        var dependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( dependencyCompilation1, dependencyCompilation2 );

        Assert.True( dependencyChanges.HasChange );
        Assert.True( dependencyChanges.HasCompileTimeCodeChange );
        Assert.Empty( dependencyChanges.ReferencedCompilationChanges );
        var dependencySyntaxTreeChange = Assert.Single( dependencyChanges.SyntaxTreeChanges ).Value;
        Assert.Equal( SyntaxTreeChangeKind.Changed, dependencySyntaxTreeChange.SyntaxTreeChangeKind );

        var dependentChanges = await compilationVersionProvider.GetCompilationChangesAsync( dependentCompilation1, dependentCompilation2 );

        Assert.True( dependentChanges.HasChange );
        Assert.True( dependentChanges.HasCompileTimeCodeChange );
        Assert.Empty( dependentChanges.SyntaxTreeChanges );
        Assert.Single( dependentChanges.ReferencedCompilationChanges );
        var referencedCompilationChange = dependentChanges.ReferencedCompilationChanges.Single().Value;
        Assert.True( referencedCompilationChange.HasCompileTimeCodeChange );
        Assert.Equal( ReferenceChangeKind.Modified, referencedCompilationChange.ChangeKind );
        Assert.Same( dependencyCompilation1, referencedCompilationChange.OldCompilationDangerous );
        Assert.Same( dependencyCompilation2, referencedCompilationChange.NewCompilation );
        Assert.NotNull( referencedCompilationChange.Changes );
        Assert.True( referencedCompilationChange.Changes.HasChange );
        Assert.True( referencedCompilationChange.Changes.HasCompileTimeCodeChange );
        Assert.Empty( referencedCompilationChange.Changes.ReferencedCompilationChanges );
        var referencedSyntaxTreeChange = Assert.Single( referencedCompilationChange.Changes.SyntaxTreeChanges ).Value;
        Assert.Equal( SyntaxTreeChangeKind.Changed, referencedSyntaxTreeChange.SyntaxTreeChangeKind );
    }

    [Fact]
    public async Task DependencyChangeConflictAddChange()
    {
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        // Dependencies form a diamond with "Top" at the top and "Dependent" at the bottom.
        var topDependencyCode1 =
            new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public partial class T {}" } };

        var topDependencyCompilation1 = TestCompilationFactory.CreateCSharpCompilation( topDependencyCode1 );

        var leftDependencyCode1 =
            new Dictionary<string, string> { { "code_left.cs", "using Metalama.Framework.Aspects; public class L : T {}" } };

        var leftDependencyCompilation1 =
            TestCompilationFactory.CreateCSharpCompilation(
                leftDependencyCode1,
                additionalReferences: new[] { topDependencyCompilation1.ToMetadataReference() } );

        var rightDependencyCode1 =
            new Dictionary<string, string> { { "code_right.cs", "using Metalama.Framework.Aspects; public class R : T {}" } };

        var rightDependencyCompilation1 =
            TestCompilationFactory.CreateCSharpCompilation(
                rightDependencyCode1,
                additionalReferences: new[] { topDependencyCompilation1.ToMetadataReference() } );

        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class D { public L X; public R Y; }" } };
        var dependentCompilation1 =
            TestCompilationFactory.CreateCSharpCompilation(
                dependentCode,
                additionalReferences: new[] { leftDependencyCompilation1.ToMetadataReference(), rightDependencyCompilation1.ToMetadataReference() } );

        // Add a syntax tree to the top dependency, which will be observed by left and right dependencies.
        var topDependencyCode2 =
            new Dictionary<string, string> { { "code_top.2.cs", "using Metalama.Framework.Aspects; public partial class T { }" } };

        var topDependencyCompilation2 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation1, topDependencyCode2 );

        var leftDependencyCompilation2 =
            TestCompilationFactory.ReplaceReferences(
                leftDependencyCompilation1,
                new[] { topDependencyCompilation2.ToMetadataReference() } );

        var rightDependencyCompilation2 =
            TestCompilationFactory.ReplaceReferences(
                rightDependencyCompilation1,
                new[] { topDependencyCompilation2.ToMetadataReference() } );

        _ = await compilationVersionProvider.GetCompilationChangesAsync( topDependencyCompilation1, topDependencyCompilation2 );
        _ = await compilationVersionProvider.GetCompilationChangesAsync( leftDependencyCompilation1, leftDependencyCompilation2 );
        _ = await compilationVersionProvider.GetCompilationChangesAsync( rightDependencyCompilation1, rightDependencyCompilation2 );

        // Change the added syntax tree. This change will be observed only by the "right" dependency.
        var topDependencyCode3 = new Dictionary<string, string> { { "code_top.2.cs", "using Metalama.Framework.Aspects; public class T { private int F; }" } };
        var topDependencyCompilation3 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation1, topDependencyCode3 );

        var rightDependencyCompilation3 =
            TestCompilationFactory.ReplaceReferences(
                rightDependencyCompilation2,
                new[] { topDependencyCompilation3.ToMetadataReference() } );

        var dependentCompilation2 =
            TestCompilationFactory.ReplaceReferences(
                dependentCompilation1,
                new[] { leftDependencyCompilation2.ToMetadataReference(), rightDependencyCompilation3.ToMetadataReference() } );

        _ = await compilationVersionProvider.GetCompilationChangesAsync( topDependencyCompilation2, topDependencyCompilation3 );
        _ = await compilationVersionProvider.GetCompilationChangesAsync( rightDependencyCompilation2, rightDependencyCompilation3 );
        _ = await compilationVersionProvider.GetCompilationChangesAsync( dependentCompilation1, dependentCompilation2 );
    }

    public static IEnumerable<object[]> DependencyChangePermutationsData
        => GetPermutations( Enumerable.Range( 0, 5 ) ).Select( s => new object[] { s.ToArray() } );

    [Theory]
    [MemberData( nameof( DependencyChangePermutationsData ) )]
    public async Task DependencyChangePermutations( int[] dependencyOrder )
    {
        // Tests different permutations of dependency changes.
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        // Initial.
        var dependencyCode1 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T {}" } };
        var dependencyCompilation1 = TestCompilationFactory.CreateCSharpCompilation( dependencyCode1, name: "dependency" );

        // Change.
        var dependencyCode2 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T { public int TT; }" } };
        var dependencyCompilation2 = TestCompilationFactory.ReplaceSyntaxTrees( dependencyCompilation1, dependencyCode2 );

        // Rename the syntax tree.
        var dependencyCode3 = new Dictionary<string, string> { { "code_top.2.cs", "using Metalama.Framework.Aspects; public class T {}" } };
        var dependencyCompilation3 = TestCompilationFactory.ReplaceSyntaxTrees( dependencyCompilation1, dependencyCode3 );

        // Change the renamed syntax tree.
        var dependencyCode4 = new Dictionary<string, string> { { "code_top.2.cs", "using Metalama.Framework.Aspects; public class T { public int TT; }" } };
        var dependencyCompilation4 = TestCompilationFactory.ReplaceSyntaxTrees( dependencyCompilation3, dependencyCode4 );

        // Rename the syntax tree back.
        var dependencyCode5 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T { public int TT; public int TTT; }" } };
        var dependencyCompilation5 = TestCompilationFactory.ReplaceSyntaxTrees( dependencyCompilation4, dependencyCode5 );

        // Initial dependent.
        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class D { public T X; }" } };

        CSharpCompilation[] compilations = [dependencyCompilation1, dependencyCompilation2, dependencyCompilation3, dependencyCompilation4, dependencyCompilation5];
        var permutation = Enumerable.Range( 0, 5 ).Select( i => compilations[dependencyOrder[i]] ).ToArray();

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var currentDependency = permutation[0];
        var currentDependent =
            TestCompilationFactory.CreateCSharpCompilation(
                dependentCode,
                name: "dependent",
                additionalReferences: new[] { currentDependency.ToMetadataReference() } );

        await compilationVersionProvider.GetCompilationVersionAsync( currentDependency );
        await compilationVersionProvider.GetCompilationVersionAsync( currentDependent );

        foreach ( var compilation in permutation.Skip( 1 ) )
        {
            var dependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentDependency, compilation );

            var dependentCompilationUpdated = TestCompilationFactory.ReplaceReferences( currentDependent, [compilation.ToMetadataReference()] );
            var dependentChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentDependent, dependentCompilationUpdated );

            // Assert that exactly the same changes are observed in the dependent compilation.
            Assert.Equal( dependencyChanges.SyntaxTreeChanges, dependentChanges.ReferencedCompilationChanges.Single().Value.Changes.SyntaxTreeChanges );

            currentDependency = compilation;
            currentDependent = dependentCompilationUpdated;
        }
    }

    public static IEnumerable<object[]> DependencyChangeTwoLevelsData
        => GetSubsets( Enumerable.Range( 1, 4 ).SelectMany( i => new[] { (i, false), (i, true) } ) ).Select( s => new object[] { s.ToArray() } );

    [Theory]
    [MemberData( nameof( DependencyChangeTwoLevelsData ) )]
    public async Task DependencyChangeTwoLevels((int Index, bool IsLeftDependency)[] topDependencyObservations)
    {
        // Tests interleavings of change observations through two dependency levels.
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        // Initial.
        var topDependencyCode1 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T {}" } };
        var topDependencyCompilation1 = TestCompilationFactory.CreateCSharpCompilation( topDependencyCode1, name: "dependency" );

        // Rename the syntax tree.
        var topDependencyCode2= new Dictionary<string, string> { { "code_top.2.cs", "using Metalama.Framework.Aspects; public class T {}" } };
        var topDependencyCompilation2 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation1, topDependencyCode2 );

        // Change renamed syntax tree.
        var topDependencyCode3 = new Dictionary<string, string> { { "code_top.2.cs", "using Metalama.Framework.Aspects; public class T { public int TT; }" } };
        var topDependencyCompilation3 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation2, topDependencyCode3 );

        // Rename the syntax tree back.
        var topDependencyCode4 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T { public int TT;" } };
        var topDependencyCompilation4 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation3, topDependencyCode4 );

        // Do another change.
        var topDependencyCode5 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T { public int TTT;" } };
        var topDependencyCompilation5 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation4, topDependencyCode5 );

        // Left.
        var leftDependencyCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class L { public T X; }" } };

        // Right.
        var rightDependencyCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class R { public T X; }" } };

        // Dependent.
        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class D { public R X; public L Y; }" } };

        CSharpCompilation[] topDependencyCompilations = [topDependencyCompilation1, topDependencyCompilation2, topDependencyCompilation3, topDependencyCompilation4, topDependencyCompilation5];

        var topDependencyObservationHashSet = topDependencyObservations.ToHashSet();

        // We will try different combinations of updating the top dependency.

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var currentTopDependency = topDependencyCompilations[0];

        var currentLeftDependency =
            TestCompilationFactory.CreateCSharpCompilation(
                leftDependencyCode,
                name: "left",
                additionalReferences: new[] { currentTopDependency.ToMetadataReference() } );

        var currentRightDependency =
            TestCompilationFactory.CreateCSharpCompilation(
                rightDependencyCode,
                name: "right",
                additionalReferences: new[] { currentTopDependency.ToMetadataReference() } );

        var currentDependent =
            TestCompilationFactory.CreateCSharpCompilation(
                dependentCode,
                name: "dependent",
                additionalReferences: new[] { currentLeftDependency.ToMetadataReference(), currentRightDependency.ToMetadataReference() } );

        // Initial compilation versions.
        await compilationVersionProvider.GetCompilationVersionAsync( currentTopDependency );
        await compilationVersionProvider.GetCompilationVersionAsync( currentLeftDependency );
        await compilationVersionProvider.GetCompilationVersionAsync( currentRightDependency );
        await compilationVersionProvider.GetCompilationVersionAsync( currentDependent );

        var currentTopDependencyForDependent = currentTopDependency;
        var currentTopDependencyForLeft = currentTopDependency;
        var currentTopDependencyForRight = currentTopDependency;

        var index = 0;
        foreach ( var topDependency in topDependencyCompilations.Skip( 1 ) )
        {
            var updateLeft = topDependencyObservationHashSet.Contains( (index, true) );
            var updateRight = topDependencyObservationHashSet.Contains( (index, false) );

            CompilationChanges? topDependencyChanges;

            if ( updateLeft || updateRight )
            {
                topDependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentTopDependency, topDependency );
            }
            else
            {
                topDependencyChanges = null;
            }

            CSharpCompilation leftDependency;
            CSharpCompilation rightDependency;

            if ( updateLeft )
            {
                leftDependency = TestCompilationFactory.ReplaceReferences( currentLeftDependency, [topDependency.ToMetadataReference()] );
                var topDependencyChangesForLeft = await compilationVersionProvider.GetCompilationChangesAsync( currentTopDependencyForLeft, topDependency );
                var leftDependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentLeftDependency, leftDependency );
                Assert.Equal( topDependencyChangesForLeft.SyntaxTreeChanges, leftDependencyChanges.ReferencedCompilationChanges.Single().Value.Changes.SyntaxTreeChanges );
            }
            else
            {
                leftDependency = currentLeftDependency;
            }

            if ( updateRight )
            {
                rightDependency = TestCompilationFactory.ReplaceReferences( currentRightDependency, [topDependency.ToMetadataReference()] );
                var topDependencyChangesForRight = await compilationVersionProvider.GetCompilationChangesAsync( currentTopDependencyForRight, topDependency );
                var rightDependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentRightDependency, rightDependency );
                Assert.Equal( topDependencyChangesForRight.SyntaxTreeChanges, rightDependencyChanges.ReferencedCompilationChanges.Single().Value.Changes.SyntaxTreeChanges );
            }
            else
            {
                rightDependency = currentRightDependency;
            }

            CSharpCompilation dependent;

            if ( updateLeft || updateRight )
            {
                dependent = TestCompilationFactory.ReplaceReferences( currentDependent, [leftDependency.ToMetadataReference(), rightDependency.ToMetadataReference()] );
                var dependentChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentDependent, dependent );
                // There is no consistent way to compare the changes in the dependent project.
            }
            else
            {
                dependent = currentDependent;
            }


            if ( updateLeft )
            {
                currentLeftDependency = leftDependency;
                currentTopDependencyForLeft = topDependency;
            }

            if ( updateRight )
            {
                currentRightDependency = rightDependency;
                currentTopDependencyForRight = topDependency;
            }

            if ( updateLeft || updateRight )
            {
                currentTopDependency = topDependency;
                currentDependent = dependent;
            }

            index++;
        }
    }

    public static IEnumerable<object[]> DependencyChangeTwoLevelsDirectData
        => GetSubsets( Enumerable.Range( 1, 3 ).SelectMany( i => new[] { (i, 0), (i, 1), (i, 2) } ) ).Select( s => new object[] { s.ToArray() } );

    [Theory]
    [MemberData( nameof( DependencyChangeTwoLevelsDirectData ) )]
    public async Task DependencyChangeTwoLevelsDirect( (int Index, int DependencyToUpdate)[] topDependencyObservations )
    {
        // Tests interleavings of change observations through two dependency levels including a direct reference to the dependent project.
        var observer = new DifferObserver();
        var mocks = new AdditionalServiceCollection( observer );
        using var testContext = this.CreateTestContext( mocks );

        // Initial.
        var topDependencyCode1 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T {}" } };
        var topDependencyCompilation1 = TestCompilationFactory.CreateCSharpCompilation( topDependencyCode1, name: "dependency" );

        // Rename the syntax tree.
        var topDdependencyCode2 = new Dictionary<string, string> { { "code_top.2.cs", "using Metalama.Framework.Aspects; public class T {}" } };
        var topDependencyCompilation2 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation1, topDdependencyCode2 );

        // Rename the syntax tree back.
        var topDependencyCode3 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T { public int TT;" } };
        var topDependencyCompilation3 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation2, topDependencyCode3 );

        // Do another change.
        var topDependencyCode4 = new Dictionary<string, string> { { "code_top.cs", "using Metalama.Framework.Aspects; public class T { public int TTT;" } };
        var topDependencyCompilation4 = TestCompilationFactory.ReplaceSyntaxTrees( topDependencyCompilation3, topDependencyCode4 );

        // Left.
        var leftDependencyCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class L { public T X; }" } };

        // Right.
        var rightDependencyCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class R { public T X; }" } };

        // Dependent.
        var dependentCode = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; public class D { public R X; public L Y; public T Z; }" } };

        CSharpCompilation[] topDependencyCompilations = [topDependencyCompilation1, topDependencyCompilation2, topDependencyCompilation3, topDependencyCompilation4];

        var topDependencyObservationHashSet = topDependencyObservations.ToHashSet();

        // We will try different combinations of updating left and right dependencies.

        var compilationVersionProvider = new ProjectVersionProvider( testContext.ServiceProvider, true );

        var currentTopDependency = topDependencyCompilations[0];

        var currentLeftDependency =
            TestCompilationFactory.CreateCSharpCompilation(
                leftDependencyCode,
                name: "left",
                additionalReferences: new[] { currentTopDependency.ToMetadataReference() } );

        var currentRightDependency =
            TestCompilationFactory.CreateCSharpCompilation(
                rightDependencyCode,
                name: "right",
                additionalReferences: new[] { currentTopDependency.ToMetadataReference() } );

        var currentDependent =
            TestCompilationFactory.CreateCSharpCompilation(
                dependentCode,
                name: "dependent",
                additionalReferences: new[] { currentLeftDependency.ToMetadataReference(), currentRightDependency.ToMetadataReference(), currentTopDependency.ToMetadataReference() } );

        // Initial compilation versions.
        await compilationVersionProvider.GetCompilationVersionAsync( currentTopDependency );
        await compilationVersionProvider.GetCompilationVersionAsync( currentLeftDependency );
        await compilationVersionProvider.GetCompilationVersionAsync( currentRightDependency );
        await compilationVersionProvider.GetCompilationVersionAsync( currentDependent );

        var currentTopDependencyForDependent = currentTopDependency;
        var currentTopDependencyForLeft = currentTopDependency;
        var currentTopDependencyForRight = currentTopDependency;

        var index = 0;
        foreach ( var topDependencyTarget in topDependencyCompilations.Skip( 1 ) )
        {
            var updateLeft = topDependencyObservationHashSet.Contains( (index, 0) );
            var updateRight = topDependencyObservationHashSet.Contains( (index, 1) );
            var updateTop = topDependencyObservationHashSet.Contains( (index, 2) );

            CompilationChanges? topDependencyChanges;

            if ( updateLeft || updateRight || updateTop )
            {
                // If anything observes the top dependency change, prepare the diff.
                topDependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentTopDependency, topDependencyTarget );
            }
            else
            {
                topDependencyChanges = null;
            }

            CSharpCompilation leftDependency;
            CSharpCompilation rightDependency;
            CSharpCompilation topDependency;

            if ( updateLeft )
            {
                leftDependency = TestCompilationFactory.ReplaceReferences( currentLeftDependency, [topDependencyTarget.ToMetadataReference()] );
                var topDependencyChangesForLeft = await compilationVersionProvider.GetCompilationChangesAsync( currentTopDependencyForLeft, topDependencyTarget );
                var leftDependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentLeftDependency, leftDependency );
                Assert.Equal( topDependencyChangesForLeft.SyntaxTreeChanges, leftDependencyChanges.ReferencedCompilationChanges.Single().Value.Changes.SyntaxTreeChanges );
            }
            else
            {
                leftDependency = currentLeftDependency;
            }

            if ( updateRight )
            {
                rightDependency = TestCompilationFactory.ReplaceReferences( currentRightDependency, [topDependencyTarget.ToMetadataReference()] );
                var topDependencyChangesForRight = await compilationVersionProvider.GetCompilationChangesAsync( currentTopDependencyForRight, topDependencyTarget );
                var rightDependencyChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentRightDependency, rightDependency );
                Assert.Equal( topDependencyChangesForRight.SyntaxTreeChanges, rightDependencyChanges.ReferencedCompilationChanges.Single().Value.Changes.SyntaxTreeChanges );
            }
            else
            {
                rightDependency = currentRightDependency;
            }

            if ( updateTop )
            {
                topDependency = topDependencyTarget;
            }
            else
            {
                topDependency = currentTopDependencyForDependent;
            }

            CSharpCompilation dependent;

            if ( updateLeft || updateRight || updateTop )
            {
                dependent = TestCompilationFactory.ReplaceReferences( currentDependent, [leftDependency.ToMetadataReference(), rightDependency.ToMetadataReference(), topDependency.ToMetadataReference()] );
                var dependentChanges = await compilationVersionProvider.GetCompilationChangesAsync( currentDependent, dependent );
                // There is no consistent way to compare the changes in the dependent project.
            }
            else
            {
                dependent = currentDependent;
            }


            if ( updateLeft )
            {
                currentLeftDependency = leftDependency;
                currentTopDependencyForLeft = topDependencyTarget;
            }

            if ( updateRight )
            {
                currentRightDependency = rightDependency;
                currentTopDependencyForRight = topDependencyTarget;
            }

            if ( updateTop )
            {
                currentTopDependencyForDependent = topDependency;
            }

            if ( updateLeft || updateRight || updateTop )
            {
                currentTopDependency = topDependencyTarget;
                currentDependent = dependent;
            }

            index++;
        }
    }

    private static async Task<WeakReference> CreateIncrementalCompilation(
        Dictionary<string, string> code,
        IEnumerable<MetadataReference> references,
        ProjectVersionProvider projectVersionProvider,
        Compilation compilation1 )
    {
        var compilation2 = TestCompilationFactory.CreateCSharpCompilation( code, name: nameof(IntermediateCompilationCanBeCollected) )
            .WithReferences( references );

        var compilationChanges2 = await projectVersionProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.True( compilationChanges2.IsIncremental );
        Assert.False( compilationChanges2.HasChange );
        Assert.False( compilationChanges2.HasCompileTimeCodeChange );

        return new WeakReference( compilation2 );
    }

    private static IEnumerable<IEnumerable<T>> GetPermutations<T>( IEnumerable<T> items, int? length = null )
    {
        length ??= items.Count();

        if ( length == 1 )
        {
            return items.Select( t => new T[] { t } );
        }

        return GetPermutations( items, length - 1 )
            .SelectMany( t => items.Where( e => !t.Contains( e ) ),
                        ( t1, t2 ) => t1.Concat( new T[] { t2 } ) );
    }

    private static IEnumerable<IEnumerable<T>> GetSubsets<T>( IEnumerable<T> source )
    {
        if ( !source.Any() )
            return new List<IEnumerable<T>> { new List<T>() };

        var element = source.First();
        var withoutElement = GetSubsets( source.Skip( 1 ) );

        var withElement = withoutElement.Select( subset => new List<T> { element }.Concat( subset ) );

        return withoutElement.Concat( withElement );
    }
}