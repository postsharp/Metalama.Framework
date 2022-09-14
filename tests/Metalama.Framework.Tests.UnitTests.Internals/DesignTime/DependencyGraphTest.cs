// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DependencyGraphTest : DesignTimeTestBase
{
    private readonly DiffStrategy _strategy = new( true, true, true );

    [Fact]
    public async Task NoChangeAsync()
    {
        using var testContext = this.CreateTestContext();
        var compilationChangesProvider = new CompilationVersionProvider( testContext.ServiceProvider );

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var masterCompilationVersion = new TestCompilationVersion( masterCompilation, hashes: new Dictionary<string, ulong> { [masterFilePath] = hash } );
        var dependencies = new BaseDependencyCollector( masterCompilationVersion );

        dependencies.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash );

        var dependentCompilationVersion = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = 0 },
            referencedCompilations: new[] { masterCompilationVersion } );

        var graph = DependencyGraph.Create( dependentCompilationVersion, dependencies );

        var changes = await DependencyChanges.IncrementalFromReferencesAsync(
            compilationChangesProvider,
            graph,
            new DesignTimeCompilationVersion( dependentCompilationVersion ) );

        Assert.True( changes.IsEmpty );
    }

    [Fact]
    public async Task FileHashChangedAsync()
    {
        using var testContext = this.CreateTestContext();
        var compilationChangesProvider = new CompilationVersionProvider( testContext.ServiceProvider );

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var masterCompilation1 = CreateCSharpCompilation( new Dictionary<string, string> { [masterFilePath] = "class C{}" }, name: "MasterAssembly" );
        var masterCompilationVersion1 = CompilationVersion.Create( masterCompilation1, this._strategy );
        var dependencyCollector = new BaseDependencyCollector( masterCompilationVersion1 );

        dependencyCollector.AddSyntaxTreeDependency(
            dependentFilePath,
            masterCompilation1.Assembly.Identity,
            masterFilePath,
            masterCompilationVersion1.SyntaxTrees.Single().Value.DeclarationHash );

        var dependentCompilationVersion1 = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = 0 },
            referencedCompilations: new[] { masterCompilationVersion1 } );

        var dependencyGraph = DependencyGraph.Create( dependentCompilationVersion1, dependencyCollector );

        // Create a second version of the master compilation with a different hash.
        var masterCompilation2 = CreateCSharpCompilation( new Dictionary<string, string> { [masterFilePath] = "class D{}" }, name: "MasterAssembly" );
        var masterCompilationVersion2 = CompilationVersion.Create( masterCompilation2, this._strategy );

        var dependentCompilationVersion2 = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = 0 },
            referencedCompilations: new[] { masterCompilationVersion2 } );

        var changes = await DependencyChanges.IncrementalFromReferencesAsync(
            compilationChangesProvider,
            dependencyGraph,
            new DesignTimeCompilationVersion( dependentCompilationVersion2 ) );

        Assert.False( changes.IsEmpty );
        Assert.Contains( dependentFilePath, changes.InvalidatedSyntaxTrees );
    }

    [Fact]
    public async Task FileHashBeenRemovedAsync()
    {
        using var testContext = this.CreateTestContext();
        var compilationChangesProvider = new CompilationVersionProvider( testContext.ServiceProvider );

        const ulong hash1 = 1;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var masterCompilationVersion1 = new TestCompilationVersion( masterCompilation, hashes: new Dictionary<string, ulong> { [masterFilePath] = hash1 } );
        var dependencyCollector = new BaseDependencyCollector( masterCompilationVersion1 );

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash1 );

        var dependentCompilationVersion1 = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = 0 },
            referencedCompilations: new[] { masterCompilationVersion1 } );

        var dependencyGraph = DependencyGraph.Create( dependentCompilationVersion1, dependencyCollector );

        // Create a second version of the master compilation with a different hash.
        var masterCompilationVersion2 = new TestCompilationVersion(
            masterCompilation,
            hashes: new Dictionary<string, ulong>
            {
                /* Intentionally empty. */
            } );

        var dependentCompilationVersion2 = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = 0 },
            referencedCompilations: new[] { masterCompilationVersion2 } );

        var changes = await DependencyChanges.IncrementalFromReferencesAsync(
            compilationChangesProvider,
            dependencyGraph,
            new DesignTimeCompilationVersion( dependentCompilationVersion2 ) );

        Assert.False( changes.IsEmpty );
        Assert.Contains( dependentFilePath, changes.InvalidatedSyntaxTrees );
    }
}