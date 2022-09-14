// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DependencyGraphTests
{
    [Fact]
    public void AddOneTree()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation ) );

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        dependencies.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Empty.Update( new[] { dependentFilePath }, dependencies );

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
        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation ) );
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencies.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation, masterFilePath, hash );
        dependencies.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Empty.Update( new[] { dependentFilePath1, dependentFilePath2 }, dependencies );

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
        var masterCompilation1 = new AssemblyIdentity( "MasterAssembly1" );
        var masterCompilation2 = new AssemblyIdentity( "MasterAssembly2" );

        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation1 ), new TestCompilationVersion( masterCompilation2 ) );

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencies.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation1, masterFilePath, hash );
        dependencies.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation2, masterFilePath, hash );

        var graph = DependencyGraph.Empty.Update( new[] { dependentFilePath1, dependentFilePath2 }, dependencies );

        Assert.Contains(
            dependentFilePath1,
            graph.DependenciesByCompilation[masterCompilation1].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );

        Assert.Contains(
            dependentFilePath2,
            graph.DependenciesByCompilation[masterCompilation2].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void AddOneTreeThenRemove()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( masterCompilation ) );
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        dependencies.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Empty
            .Update( new[] { dependentFilePath }, dependencies )
            .Update( new[] { dependentFilePath }, BaseDependencyCollector.Empty );

        Assert.Empty( graph.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath );
    }

    [Fact]
    public void AddTwoDependentTreesInSameCompilationThenRemoveOne()
    {
        var dependencies = new BaseDependencyCollector();
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencies.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation, masterFilePath, hash );
        dependencies.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Empty
            .Update( new[] { dependentFilePath1, dependentFilePath2 }, dependencies )
            .Update( new[] { dependentFilePath2 }, BaseDependencyCollector.Empty );

        var dependenciesByCompilation = graph.DependenciesByCompilation.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.AssemblyIdentity );

        Assert.Contains(
            dependentFilePath1,
            graph.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );

        Assert.DoesNotContain(
            dependentFilePath2,
            graph.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void UpdateSyntaxTreeHash()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        const ulong hash1 = 54;
        const ulong hash2 = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var dependencies1 = new BaseDependencyCollector();
        dependencies1.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash1 );

        var graph1 = DependencyGraph.Empty.Update( new[] { dependentFilePath }, dependencies1 );

        var dependencies2 = new BaseDependencyCollector();
        dependencies2.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash2 );

        var graph2 = graph1.Update( new[] { dependentFilePath }, dependencies2 );

        Assert.Equal( hash2, graph2.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DeclarationHash );
    }
}