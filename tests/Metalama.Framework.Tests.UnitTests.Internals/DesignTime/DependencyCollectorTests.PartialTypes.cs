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

public partial class DependencyCollectorTests : TestBase
{
    [Fact]
    public void AddOnePartialTypeDependency()
    {
        var assemblyIdentity = new AssemblyIdentity( "DependentAssembly" );
        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( assemblyIdentity ) );

        const string dependentFilePath = "dependent.cs";
        var masterType = new TypeDependencyKey( "type" );
        dependencies.AddPartialTypeDependency( dependentFilePath, assemblyIdentity, masterType );

        Assert.Contains(
            masterType,
            dependencies.DependenciesByDependentFilePath[dependentFilePath]
                .DependenciesByCompilation.Values.Single()
                .MasterPartialTypes );
    }

    [Fact]
    public void AddDuplicatePartialDependency()
    {
        var assemblyIdentity = new AssemblyIdentity( "DependentAssembly" );
        var dependencies = new BaseDependencyCollector( new TestCompilationVersion( assemblyIdentity ) );

        const string dependentFilePath = "dependent.cs";
        var masterType = new TypeDependencyKey( "type" );
        dependencies.AddPartialTypeDependency( dependentFilePath, assemblyIdentity, masterType );
        dependencies.AddPartialTypeDependency( dependentFilePath, assemblyIdentity, masterType );

        Assert.Contains(
            masterType,
            dependencies.DependenciesByDependentFilePath[dependentFilePath]
                .DependenciesByCompilation.Values.Single()
                .MasterPartialTypes );
    }

    [Fact]
    public void CollectPartialTypeDependenciesWithinProject()
    {
        using var testContext = this.CreateTestContext();

        var code = new Dictionary<string, string>
        {
            ["Class1_1.cs"] = "partial class Class1 { }",
            ["Class1_2.cs"] = "partial class Class1 { }",
            ["Class2_1.cs"] = "partial class Class2 : Class1 { }",
            ["Class2_2.cs"] = "partial class Class2 { }",
            ["Interface1_1.cs"] = "partial interface Interface1 { }",
            ["Interface1_2.cs"] = "partial interface Interface1 { }",
            ["Interface2.cs"] = "interface Interface3 : Interface1 { }",
            ["Class3.cs"] = "class Class3 : Class2, Interface3 { }"
        };

        var compilation = CreateCSharpCompilation( code );

        var dependencyCollector = new DependencyCollector( testContext.ServiceProvider, compilation, Enumerable.Empty<ICompilationVersion>() );

        var partialCompilation = PartialCompilation.CreatePartial( compilation, compilation.SyntaxTrees );
        partialCompilation.DerivedTypes.PopulateDependencies( dependencyCollector );

        var actualDependencies = string.Join(
            Environment.NewLine,
            dependencyCollector.EnumeratePartialTypeDependencies().Select( x => $"'{x.MasterType}'->'{x.DependentFilePath}'" ).OrderBy( x => x ) );

        var expectedDependencies = @"'Class1'->'Class2_1.cs'
'Class1'->'Class2_2.cs'
'Class1'->'Class3.cs'
'Class2'->'Class3.cs'
'Interface1'->'Class3.cs'
'Interface1'->'Interface2.cs'";

        Assert.Equal( expectedDependencies, actualDependencies );
    }

    [Fact]
    public void CollectPartialTypeDependenciesAcrossProjects()
    {
        using var testContext = this.CreateTestContext();

        var code1 = new Dictionary<string, string>
        {
            ["Interface1_1.cs"] = "public partial interface Interface1 { }",
            ["Interface1_2.cs"] = "partial interface Interface1 { }",
            ["Interface2.cs"] = "public interface Interface2 : Interface1 { }"
        };

        var compilation1 = CreateCSharpCompilation( code1 );

        var code2 = new Dictionary<string, string>
        {
            ["Class1_1.cs"] = "partial class Class1 { }",
            ["Class1_2.cs"] = "partial class Class1 { }",
            ["Class2_1.cs"] = "partial class Class2 : Class1 { }",
            ["Class2_2.cs"] = "partial class Class2 { }",
            ["Class3.cs"] = "class Class3 : Class2, Interface2 { }"
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
            dependencyCollector.EnumeratePartialTypeDependencies().Select( x => $"'{x.MasterType}'->'{x.DependentFilePath}'" ).OrderBy( x => x ) );

        var expectedDependencies = @"'Class1'->'Class2_1.cs'
'Class1'->'Class2_2.cs'
'Class1'->'Class3.cs'
'Class2'->'Class3.cs'
'Interface1'->'Class3.cs'";

        Assert.Equal( expectedDependencies, actualDependencies );
    }
}