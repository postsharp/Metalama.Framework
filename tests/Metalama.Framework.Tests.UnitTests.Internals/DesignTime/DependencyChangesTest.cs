// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DependencyChangesTest : TestBase
{
    [Fact]
    public async Task NoChange()
    {
        using var testContext = this.CreateTestContext();
        var compilationChangesProvider = new CompilationChangesProvider(testContext.ServiceProvider);

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var masterCompilationVersion = new TestCompilationVersion( masterCompilation, hashes: new Dictionary<string, ulong> { [masterFilePath] = hash } );
        var dependencies = new BaseDependencyCollector( masterCompilationVersion );

        dependencies.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Empty.Update( new[] { dependentFilePath }, dependencies );

        var changes = await DependencyChanges.IncrementalFromReferencesAsync(
            compilationChangesProvider,
            graph,
            new DesignTimeCompilationReferenceCollection( new DesignTimeCompilationReference( masterCompilationVersion ) ) );

        Assert.True( changes.IsEmpty );
    }

    [Fact]
    public async Task FileHashChanged()
    {
        using var testContext = this.CreateTestContext();
        var compilationChangesProvider = new CompilationChangesProvider(testContext.ServiceProvider);
        
        const ulong hash1 = 1;
        const ulong hash2 = 2;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var masterCompilationVersion1 = new TestCompilationVersion( masterCompilation, hashes: new Dictionary<string, ulong> { [masterFilePath] = hash1 } );
        var dependencyCollector = new BaseDependencyCollector( masterCompilationVersion1 );

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash1 );

        var dependencyGraph = DependencyGraph.Empty.Update( new[] { dependentFilePath }, dependencyCollector );

        // Create a second version of the master compilation with a different hash.
        var masterCompilationVersion2 = new TestCompilationVersion( masterCompilation, hashes: new Dictionary<string, ulong> { [masterFilePath] = hash2 } );

        var changes = await DependencyChanges.IncrementalFromReferencesAsync(
            compilationChangesProvider,
            dependencyGraph,
            new DesignTimeCompilationReferenceCollection( new DesignTimeCompilationReference( masterCompilationVersion2 ) ) );

        Assert.False( changes.IsEmpty );
        Assert.Contains( dependentFilePath, changes.InvalidatedSyntaxTrees );
    }

    [Fact]
    public async Task FileHashBeenRemoved()
    {
        using var testContext = this.CreateTestContext();
        var compilationChangesProvider = new CompilationChangesProvider(testContext.ServiceProvider);

        const ulong hash1 = 1;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var masterCompilationVersion1 = new TestCompilationVersion( masterCompilation, hashes: new Dictionary<string, ulong> { [masterFilePath] = hash1 } );
        var dependencyCollector = new BaseDependencyCollector( masterCompilationVersion1 );

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash1 );

        var dependencyGraph = DependencyGraph.Empty.Update( new[] { dependentFilePath }, dependencyCollector );

        // Create a second version of the master compilation with a different hash.
        var masterCompilationVersion2 = new TestCompilationVersion(
            masterCompilation,
            hashes: new Dictionary<string, ulong>
            {
                /* Intentionally empty. */
            } );

        var changes = await DependencyChanges.IncrementalFromReferencesAsync(
            compilationChangesProvider,
            dependencyGraph,
            new DesignTimeCompilationReferenceCollection( new DesignTimeCompilationReference( masterCompilationVersion2 ) ) );

        Assert.False( changes.IsEmpty );
        Assert.Contains( dependentFilePath, changes.InvalidatedSyntaxTrees );
    }
}