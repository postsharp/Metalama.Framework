// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DependencyGraphTests
{
    [Fact]
    public void AddOneTree()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var dependencyCollector = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation ) );

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash );

        var dependentCompilationVersion = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = 0 } );

        var graph = DependencyGraph.Create( dependentCompilationVersion, dependencyCollector );

        var dependenciesByCompilation = graph.DependenciesByCompilation.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.AssemblyIdentity );
        var dependenciesByMasterFile = graph.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath.Values.Single();
        Assert.Equal( masterFilePath, dependenciesByMasterFile.FilePath );
        Assert.Equal( hash, dependenciesByMasterFile.DeclarationHash );
    }

    [Fact]
    public void AddTwoDependentTreesInSameCompilation()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var dependencyCollector = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation ) );
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation, masterFilePath, hash );
        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation, masterFilePath, hash );

        var dependentCompilationVersion = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath1] = hash, [dependentFilePath2] = hash } );

        var graph = DependencyGraph.Create( dependentCompilationVersion, dependencyCollector );

        var dependenciesByCompilation = graph.DependenciesByCompilation.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.AssemblyIdentity );

        Assert.Contains(
            dependentFilePath1,
            graph.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );

        Assert.Contains(
            dependentFilePath2,
            graph.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void AddTwoDependentTreesInDifferentCompilation()
    {
        var masterCompilation1 = new TestCompilationVersion( "MasterAssembly1" );
        var masterCompilation2 = new TestCompilationVersion( "MasterAssembly2" );

        var dependentCompilation = new TestCompilationVersion(
            "Dependent",
            referencedCompilations: new ICompilationVersion[] { masterCompilation1, masterCompilation2 } );

        var dependencyCollector = new BaseDependencyCollector( dependentCompilation );

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation1.AssemblyIdentity, masterFilePath, hash );
        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation2.AssemblyIdentity, masterFilePath, hash );

        var dependentCompilationVersion = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath1] = hash, [dependentFilePath2] = hash } );

        var graph = DependencyGraph.Create( dependentCompilationVersion, dependencyCollector );

        Assert.Contains(
            dependentFilePath1,
            graph.DependenciesByCompilation[masterCompilation1.AssemblyIdentity].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );

        Assert.Contains(
            dependentFilePath2,
            graph.DependenciesByCompilation[masterCompilation2.AssemblyIdentity].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void AddOneTreeThenRemoveDependency()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var dependencies1 = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation ) );
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        dependencies1.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash );

        var dependentCompilationVersion1 = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = 0 } );

        var graph1 = DependencyGraph.Create( dependentCompilationVersion1, dependencies1 );

        var dependencies2 = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation ) );
        var graph2 = graph1.Update( dependentCompilationVersion1, dependencies2 );

        Assert.Empty( graph2.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath );
    }

    [Fact]
    public void AddTwoDependentTreesInSameCompilationThenRemoveOne()
    {
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        var masterCompilation = new TestCompilationVersion( "MasterAssembly" );

        // Create a 1st version of the dependent assembly with two references to master.cs.
        var dependentCompilationVersion1 = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath1] = hash, [dependentFilePath2] = hash },
            referencedCompilations: new ICompilationVersion[] { masterCompilation } );

        var dependencyCollector1 = new BaseDependencyCollector( dependentCompilationVersion1 );

        dependencyCollector1.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation.AssemblyIdentity, masterFilePath, hash );
        dependencyCollector1.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation.AssemblyIdentity, masterFilePath, hash );

        var graph1 = DependencyGraph.Create( dependentCompilationVersion1, dependencyCollector1 );

        // Create a 1st version of the dependent assembly with just 1 reference to master.cs.
        var dependencyCollector2 = new BaseDependencyCollector( dependentCompilationVersion1 );
        dependencyCollector2.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation.AssemblyIdentity, masterFilePath, hash );

        var graph2 = graph1
            .Update( dependentCompilationVersion1, dependencyCollector2 );

        var dependenciesByCompilation = graph2.DependenciesByCompilation.Values.Single();
        Assert.Equal( masterCompilation.AssemblyIdentity, dependenciesByCompilation.AssemblyIdentity );

        Assert.Contains(
            dependentFilePath1,
            graph2.DependenciesByCompilation[masterCompilation.AssemblyIdentity].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );

        Assert.DoesNotContain(
            dependentFilePath2,
            graph2.DependenciesByCompilation[masterCompilation.AssemblyIdentity].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void UpdateSyntaxTreeHash()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        const ulong hash1 = 54;
        const ulong hash2 = 55;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var dependencies1 = new BaseDependencyCollector( new TestCompilationVersion( "dummy" ) );
        dependencies1.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash1 );

        var dependentCompilationVersion1 = new TestCompilationVersion(
            new AssemblyIdentity( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath] = hash1 } );

        var graph1 = DependencyGraph.Create( dependentCompilationVersion1, dependencies1 );

        var dependencies2 = new BaseDependencyCollector( new TestCompilationVersion( "dummy" ) );
        dependencies2.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash2 );

        var graph2 = graph1.Update( dependentCompilationVersion1, dependencies2 );

        Assert.Equal( hash2, graph2.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DeclarationHash );
    }
}