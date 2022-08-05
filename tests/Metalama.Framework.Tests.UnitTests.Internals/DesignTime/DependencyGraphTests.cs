// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
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

        var dependenciesByCompilation = graph.Compilations.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.AssemblyIdentity );
        var dependenciesByMasterFile = graph.Compilations[masterCompilation].DependenciesByMasterFilePath.Values.Single();
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

        var dependenciesByCompilation = graph.Compilations.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.AssemblyIdentity );
        Assert.Contains( dependentFilePath1, graph.Compilations[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
        Assert.Contains( dependentFilePath2, graph.Compilations[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
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

        Assert.Contains( dependentFilePath1, graph.Compilations[masterCompilation1].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
        Assert.Contains( dependentFilePath2, graph.Compilations[masterCompilation2].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
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

        Assert.Empty( graph.Compilations[masterCompilation].DependenciesByMasterFilePath );
    }

    [Fact]
    public void AddTwoDependentTreesInSameCompilationThenRemoveOne()
    {
        var dependencies = new BaseDependencyCollector();
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var references = new DesignTimeCompilationReferenceCollection( new DesignTimeCompilationReference( new TestCompilationVersion( masterCompilation ) ) );
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencies.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation, masterFilePath, hash );
        dependencies.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Empty
            .Update( new[] { dependentFilePath1, dependentFilePath2 }, dependencies )
            .Update( new[] { dependentFilePath2 }, BaseDependencyCollector.Empty );

        var dependenciesByCompilation = graph.Compilations.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.AssemblyIdentity );
        Assert.Contains( dependentFilePath1, graph.Compilations[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
        Assert.DoesNotContain( dependentFilePath2, graph.Compilations[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void UpdateSyntaxTreeHash()
    {
        var masterCompilation = new AssemblyIdentity( "MasterAssembly" );
        var references = new DesignTimeCompilationReferenceCollection( new DesignTimeCompilationReference( new TestCompilationVersion( masterCompilation ) ) );
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

        Assert.Equal( hash2, graph2.Compilations[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DeclarationHash );
    }
}