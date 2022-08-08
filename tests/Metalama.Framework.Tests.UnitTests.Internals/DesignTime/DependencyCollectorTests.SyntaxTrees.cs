// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public partial class DependencyCollectorTests
{
    [Fact]
    public void AddOneSyntaxTreeDependency()
    {
        var assemblyIdentity = new AssemblyIdentity( "DependentAssembly" );
        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( assemblyIdentity ) );
        const ulong hash = 54;

        const string dependentFilePath = "dependent.cs";
        const string masterFilePath = "master.cs";
        dependencies.AddSyntaxTreeDependency( dependentFilePath, assemblyIdentity, masterFilePath, hash );

        Assert.Equal( dependentFilePath, dependencies.DependenciesByDependentFilePath[dependentFilePath].DependentFilePath );

        Assert.Equal(
            hash,
            dependencies.DependenciesByDependentFilePath[dependentFilePath]
                .DependenciesByCompilation.Values.Single()
                .MasterFilePathsAndHashes[masterFilePath] );
    }

    [Fact]
    public void AddDuplicateSyntaxTreeDependency()
    {
        var assemblyIdentity = new AssemblyIdentity( "DependentAssembly" );
        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( assemblyIdentity ) );

        const ulong hash = 54;

        const string dependentFilePath = "dependent.cs";
        const string masterFilePath = "master.cs";
        dependencies.AddSyntaxTreeDependency( dependentFilePath, assemblyIdentity, masterFilePath, hash );
        dependencies.AddSyntaxTreeDependency( dependentFilePath, assemblyIdentity, masterFilePath, hash );

        Assert.Equal( dependentFilePath, dependencies.DependenciesByDependentFilePath[dependentFilePath].DependentFilePath );

        Assert.Equal(
            hash,
            dependencies.DependenciesByDependentFilePath[dependentFilePath]
                .DependenciesByCompilation.Values.Single()
                .MasterFilePathsAndHashes[masterFilePath] );
    }

    [Fact]
    public void CollectSyntaxTreeDependenciesWithinProject()
    {
        using var testContext = this.CreateTestContext();

        var code = new Dictionary<string, string>
        {
            ["Class1.cs"] = "public class Class1 { }",
            ["Class2.cs"] = "public class Class2 { }",
            ["Class3.cs"] = "public class Class3 : Class2 { }",
            ["Interface1.cs"] = "public interface Interface1 { }",
            ["Interface2.cs"] = "public interface Interface2 : Interface1 { }",
            ["Interface3.cs"] = "public interface Interface3 : Interface2 { }",
            ["Class4.cs"] = "public class Class4 : Class3, Interface3 { }"
        };

        var compilation = CreateCSharpCompilation( code );

        var dependencyCollector = new DependencyCollector( testContext.ServiceProvider, compilation, Enumerable.Empty<ICompilationVersion>() );

        var partialCompilation = PartialCompilation.CreatePartial( compilation, compilation.SyntaxTrees );
        partialCompilation.DerivedTypes.PopulateDependencies( dependencyCollector );

        var actualDependencies = string.Join(
            Environment.NewLine,
            dependencyCollector.EnumerateSyntaxTreeDependencies().Select( x => $"'{x.MasterFilePath}'->'{x.DependentFilePath}'" ).OrderBy( x => x ) );

        var expectedDependencies = @"'Class2.cs'->'Class3.cs'
'Class2.cs'->'Class4.cs'
'Class3.cs'->'Class4.cs'
'Interface1.cs'->'Class4.cs'
'Interface1.cs'->'Interface2.cs'
'Interface1.cs'->'Interface3.cs'
'Interface2.cs'->'Class4.cs'
'Interface2.cs'->'Interface3.cs'
'Interface3.cs'->'Class4.cs'";

        Assert.Equal( expectedDependencies, actualDependencies );
    }

    [Fact]
    public void CollectSyntaxTreeDependenciesAcrossProject()
    {
        using var testContext = this.CreateTestContext();

        var code1 = new Dictionary<string, string>
        {
            ["Interface1.cs"] = "public interface Interface1 { }",
            ["Interface2.cs"] = "public interface Interface2 : Interface1 { }",
            ["Interface3.cs"] = "public interface Interface3 : Interface2 { }"
        };

        var compilation1 = CreateCSharpCompilation( code1 );

        var code2 = new Dictionary<string, string>
        {
            ["Class1.cs"] = "public class Class1 { }",
            ["Class2.cs"] = "public class Class2 { }",
            ["Class3.cs"] = "public class Class3 : Class2 { }",
            ["Class4.cs"] = "public class Class4 : Class3, Interface3 { }"
        };

        var compilation2 = CreateCSharpCompilation( code2, additionalReferences: new[] { compilation1.ToMetadataReference() } );

        var dependencyCollector = new DependencyCollector(
            testContext.ServiceProvider,
            compilation2,
            new ICompilationVersion[] { CompilationVersion.Create( compilation1, new DiffStrategy( true, true, true ) ) } );

        var partialCompilation = PartialCompilation.CreatePartial( compilation2, compilation2.SyntaxTrees );
        partialCompilation.DerivedTypes.PopulateDependencies( dependencyCollector );

        var actualDependencies = string.Join(
            Environment.NewLine,
            dependencyCollector.EnumerateSyntaxTreeDependencies().Select( x => $"'{x.MasterFilePath}'->'{x.DependentFilePath}'" ).OrderBy( x => x ) );

        var expectedDependencies = @"'Class2.cs'->'Class3.cs'
'Class2.cs'->'Class4.cs'
'Class3.cs'->'Class4.cs'
'Interface1.cs'->'Class4.cs'
'Interface2.cs'->'Class4.cs'
'Interface3.cs'->'Class4.cs'";

        Assert.Equal( expectedDependencies, actualDependencies );
    }
}