// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DependencyGraphTests : DesignTimeTestBase
{
    [Fact]
    public void AddOneTree()
    {
        var masterCompilation = ProjectKey.CreateTest( "MasterAssembly" );
        var dependencyCollector = new BaseDependencyCollector( new TestProjectVersion( masterCompilation ) );

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Create( dependencyCollector );

        var dependenciesByCompilation = graph.DependenciesByCompilation.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.ProjectKey );
        var dependenciesByMasterFile = graph.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath.Values.Single();
        Assert.Equal( masterFilePath, dependenciesByMasterFile.FilePath );
        Assert.Equal( hash, dependenciesByMasterFile.DeclarationHash );
    }

    [Fact]
    public void AddTwoDependentTreesInSameCompilation()
    {
        var masterCompilation = ProjectKey.CreateTest( "MasterAssembly" );
        var dependencyCollector = new BaseDependencyCollector( new TestProjectVersion( masterCompilation ) );
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation, masterFilePath, hash );
        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation, masterFilePath, hash );

        var graph = DependencyGraph.Create( dependencyCollector );

        var dependenciesByCompilation = graph.DependenciesByCompilation.Values.Single();
        Assert.Equal( masterCompilation, dependenciesByCompilation.ProjectKey );

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
        var masterCompilation1 = new TestProjectVersion( "MasterAssembly1" );
        var masterCompilation2 = new TestProjectVersion( "MasterAssembly2" );

        var dependentCompilation = new TestProjectVersion(
            "Dependent",
            referencedCompilations: new IProjectVersion[] { masterCompilation1, masterCompilation2 } );

        var dependencyCollector = new BaseDependencyCollector( dependentCompilation );

        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation1.ProjectKey, masterFilePath, hash );
        dependencyCollector.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation2.ProjectKey, masterFilePath, hash );

        var graph = DependencyGraph.Create( dependencyCollector );

        Assert.Contains(
            dependentFilePath1,
            graph.DependenciesByCompilation[masterCompilation1.ProjectKey].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );

        Assert.Contains(
            dependentFilePath2,
            graph.DependenciesByCompilation[masterCompilation2.ProjectKey].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void AddOneTreeThenRemoveDependency()
    {
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var masterCompilation = CreateCSharpCompilation(
            new Dictionary<string, string>() { [masterFilePath] = "", [dependentFilePath] = "" },
            name: "MasterAssembly" );

        var dependencies1 = new BaseDependencyCollector( new TestProjectVersion( masterCompilation ) );

        dependencies1.AddSyntaxTreeDependency( dependentFilePath, masterCompilation.GetProjectKey(), masterFilePath, hash );

        var graph1 = DependencyGraph.Create( dependencies1 );

        var dependencies2 = new BaseDependencyCollector( new TestProjectVersion( masterCompilation ) );
        var graph2 = graph1.Update( dependencies2 );

        Assert.Empty( graph2.DependenciesByCompilation );
    }

    [Fact]
    public void AddTwoDependentTreesInSameCompilationThenRemoveOne()
    {
        const ulong hash = 54;

        const string masterFilePath = "master.cs";
        const string dependentFilePath1 = "dependent1.cs";
        const string dependentFilePath2 = "dependent2.cs";

        var masterCompilation = new TestProjectVersion( "MasterAssembly" );

        // Create a 1st version of the dependent assembly with two references to master.cs.
        var dependentCompilationVersion1 = new TestProjectVersion(
            ProjectKey.CreateTest( "DependentAssembly" ),
            hashes: new Dictionary<string, ulong>() { [dependentFilePath1] = hash, [dependentFilePath2] = hash },
            referencedCompilations: new IProjectVersion[] { masterCompilation } );

        var dependencyCollector1 = new BaseDependencyCollector( dependentCompilationVersion1 );

        dependencyCollector1.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation.ProjectKey, masterFilePath, hash );
        dependencyCollector1.AddSyntaxTreeDependency( dependentFilePath2, masterCompilation.ProjectKey, masterFilePath, hash );

        var graph1 = DependencyGraph.Create( dependencyCollector1 );

        // Create a 1st version of the dependent assembly with just 1 reference to master.cs.
        var dependencyCollector2 = new BaseDependencyCollector( dependentCompilationVersion1 );
        dependencyCollector2.AddSyntaxTreeDependency( dependentFilePath1, masterCompilation.ProjectKey, masterFilePath, hash );

        var graph2 = graph1
            .Update( dependencyCollector2 );

        var dependenciesByCompilation = graph2.DependenciesByCompilation.Values.Single();
        Assert.Equal( masterCompilation.ProjectKey, dependenciesByCompilation.ProjectKey );

        Assert.Contains(
            dependentFilePath1,
            graph2.DependenciesByCompilation[masterCompilation.ProjectKey].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );

        Assert.DoesNotContain(
            dependentFilePath2,
            graph2.DependenciesByCompilation[masterCompilation.ProjectKey].DependenciesByMasterFilePath[masterFilePath].DependentFilePaths );
    }

    [Fact]
    public void UpdateSyntaxTreeHash()
    {
        var masterCompilation = ProjectKey.CreateTest( "MasterAssembly" );
        const ulong hash1 = 54;
        const ulong hash2 = 55;

        const string masterFilePath = "master.cs";
        const string dependentFilePath = "dependent.cs";

        var dependencies1 = new BaseDependencyCollector( new TestProjectVersion( "dummy" ) );
        dependencies1.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash1 );

        var graph1 = DependencyGraph.Create( dependencies1 );

        var dependencies2 = new BaseDependencyCollector( new TestProjectVersion( "dummy" ) );
        dependencies2.AddSyntaxTreeDependency( dependentFilePath, masterCompilation, masterFilePath, hash2 );

        var graph2 = graph1.Update( dependencies2 );

        Assert.Equal( hash2, graph2.DependenciesByCompilation[masterCompilation].DependenciesByMasterFilePath[masterFilePath].DeclarationHash );
    }
}