// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class CompilationChangesProviderTests : TestBase
{
    [Fact]
    public async Task DifferentCompilationWithNoChangeAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var graph = new CompilationChangesProvider( testContext.ServiceProvider.WithService( observer ) );
        var compilation1 = CreateCSharpCompilation( code );
        var compilationChanges1 = await graph.GetCompilationChangesAsync( null, compilation1 );
        Assert.Same( compilation1, compilationChanges1.NewCompilationVersion.Compilation );
        Assert.False( compilationChanges1.IsIncremental );
        Assert.True( compilationChanges1.HasChange );
        Assert.True( compilationChanges1.HasCompileTimeCodeChange );
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 1, observer.UpdateCompilationVersionEventCount );
        Assert.Equal( 0, observer.MergeCompilationChangesEventCount );

        // Create a second compilation. We explicitly copy the references from the first compilation because references
        // may not be equal due to a change in the loaded assemblies in the AppDomain during the execution of the test.
        var compilation2 = CreateCSharpCompilation( code ).WithReferences( compilation1.References );
        var compilationChanges2 = await graph.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.True( compilationChanges2.IsIncremental );
        Assert.False( compilationChanges2.HasChange );
        Assert.False( compilationChanges2.HasCompileTimeCodeChange );
        Assert.Same( compilation1, compilationChanges2.NewCompilationVersion.Compilation ); // There is no change, so compilation1 is expected.
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 2, observer.UpdateCompilationVersionEventCount );
        Assert.Equal( 0, observer.MergeCompilationChangesEventCount );

        // Calling GetCompilationVersion a second time with the same parameter should not trigger an UpdateCompilationVersion.
        _ = await graph.GetCompilationChangesAsync( compilation1, compilation2 );
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 2, observer.UpdateCompilationVersionEventCount );
        Assert.Equal( 0, observer.MergeCompilationChangesEventCount );

        // Create a third compilation with no change.
        var compilation3 = CreateCSharpCompilation( code ).WithReferences( compilation1.References );
        var compilationChanges3 = await graph.GetCompilationChangesAsync( compilation1, compilation3 );
        Assert.True( compilationChanges3.IsIncremental );
        Assert.False( compilationChanges3.HasChange );
        Assert.False( compilationChanges3.HasCompileTimeCodeChange );
        Assert.Same( compilation1, compilationChanges3.NewCompilationVersion.Compilation ); // There is no change, so compilation1 is expected.
        Assert.Equal( 1, observer.NewCompilationEventCount );
        Assert.Equal( 3, observer.UpdateCompilationVersionEventCount );
        Assert.Equal( 0, observer.MergeCompilationChangesEventCount );
    }

    [Fact]
    public async Task SameCompilationWithoutPredecessorAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var graph = new CompilationChangesProvider( testContext.ServiceProvider.WithService( observer ) );
        var compilation1 = CreateCSharpCompilation( code );
        _ = await graph.GetCompilationChangesAsync( null, compilation1 );
        _ = await graph.GetCompilationChangesAsync( null, compilation1 );
    }

    [Fact]
    public async Task SameCompilationWithDifferentPredecessorAsync()
    {
        var code = new Dictionary<string, string> { ["code.cs"] = "class C {}" };

        using var testContext = this.CreateTestContext();
        var observer = new DifferObserver();

        var graph = new CompilationChangesProvider( testContext.ServiceProvider.WithService( observer ) );
        var compilation1 = CreateCSharpCompilation( code );
        var compilation2 = CreateCSharpCompilation( code );
        var compilation3 = CreateCSharpCompilation( code );
        var compilationVersion3a = await graph.GetCompilationChangesAsync( compilation1, compilation3 );
        var compilationVersion3b = await graph.GetCompilationChangesAsync( compilation2, compilation3 );
    }
}