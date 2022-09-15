﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

#pragma warning disable VSTHRD200 // Async method names must have "Async" suffix.

public class DependencyGraphInvalidationTests : DesignTimeTestBase
{
    private async Task<( List<string> InvalidatedSyntaxTrees, DependencyGraph DependenciesBefore, DependencyGraph DependenciesAfter )>
        ProcessCompilationChangesAsync(
            Dictionary<string, string> codeBefore,
            Dependency[] dependencies,
            Dictionary<string, string> codeAfter )
    {
        using var testContext = this.CreateTestContext();
        var compilationChangesProvider = new CompilationVersionProvider( testContext.ServiceProvider );

        var compilation1 = CreateCSharpCompilation( codeBefore, name: "test", ignoreErrors: true );
        var compilationVersion1 = await compilationChangesProvider.GetCompilationVersionAsync( compilation1 );
        var dependencyCollector = new BaseDependencyCollector( compilationVersion1 );

        foreach ( var dependency in dependencies )
        {
            if ( dependency.Kind == DependencyKind.SyntaxTree )
            {
                dependencyCollector.AddSyntaxTreeDependency(
                    dependency.Dependent,
                    compilation1.Assembly.Identity,
                    dependency.Master,
                    compilationVersion1.SyntaxTrees[dependency.Master].DeclarationHash );
            }
            else
            {
                dependencyCollector.AddPartialTypeDependency(
                    dependency.Dependent,
                    compilation1.Assembly.Identity,
                    new TypeDependencyKey( dependency.Master ) );
            }
        }

        var dependencyGraph = DependencyGraph.Create( dependencyCollector );

        var compilation2 = CreateCSharpCompilation( codeAfter, name: "test", ignoreErrors: true );

        var changes = await compilationChangesProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        var invalidatedSyntaxTrees = new HashSet<string>();

        var newDependencyGraph =
            await compilationChangesProvider.ProcessCompilationChangesAsync( changes, dependencyGraph, t => invalidatedSyntaxTrees.Add( t ), true );

        return (invalidatedSyntaxTrees.OrderBy( x => x ).ToList(), dependencyGraph, newDependencyGraph);
    }

    private record Dependency( DependencyKind Kind, string Master, string Dependent );

    private enum DependencyKind
    {
        SyntaxTree,
        PartialType
    }

    [Fact]
    public async Task NoChange()
    {
        const string masterFile = "master.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile] = "class C {}", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.SyntaxTree, masterFile, dependentFile ) };
        var codeAfter = codeBefore;

        var result = await this.ProcessCompilationChangesAsync( codeBefore, dependencies, codeAfter );

        Assert.Empty( result.InvalidatedSyntaxTrees );
        Assert.Same( result.DependenciesBefore.DependenciesByCompilation, result.DependenciesAfter.DependenciesByCompilation );
    }

    [Fact]
    public async Task SyntaxTreeDependency_MasterFileChanged()
    {
        const string masterFile = "master.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile] = "class C {}", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.SyntaxTree, masterFile, dependentFile ) };
        var codeAfter = new Dictionary<string, string> { [masterFile] = "class C { void M() {} }", [dependentFile] = "class D : C {}" };

        var result = await this.ProcessCompilationChangesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( result.InvalidatedSyntaxTrees, dependentFile );
        Assert.Same( result.DependenciesBefore.DependenciesByCompilation, result.DependenciesAfter.DependenciesByCompilation );
    }

    [Fact]
    public async Task SyntaxTreeDependency_MasterFileRemoved()
    {
        const string masterFile = "master.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile] = "class C {}", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.SyntaxTree, masterFile, dependentFile ) };
        var codeAfter = new Dictionary<string, string> { [dependentFile] = "class D : C {}" };

        var result = await this.ProcessCompilationChangesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( result.InvalidatedSyntaxTrees, dependentFile );
        Assert.Same( result.DependenciesBefore.DependenciesByCompilation, result.DependenciesAfter.DependenciesByCompilation );
    }

    [Fact]
    public async Task SyntaxTreeDependency_DependentFileRemoved()
    {
        const string masterFile = "master.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile] = "class C {}", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.SyntaxTree, masterFile, dependentFile ) };
        var codeAfter = new Dictionary<string, string> { [masterFile] = "class C {}" };

        var result = await this.ProcessCompilationChangesAsync( codeBefore, dependencies, codeAfter );

        Assert.Empty( result.InvalidatedSyntaxTrees );
        Assert.Single( result.DependenciesBefore.DependenciesByCompilation );
        Assert.Empty( result.DependenciesAfter.DependenciesByCompilation );
    }

    [Fact]
    public async Task PartialTypeDependency_FileWithPartialTypeAdded()
    {
        const string masterFile1 = "master1.cs";
        const string masterFile2 = "master2.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile1] = "partial class C {}", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.PartialType, "C", dependentFile ) };

        var codeAfter = new Dictionary<string, string>
        {
            [masterFile1] = "partial class C {}", [masterFile2] = "partial class C {}", [dependentFile] = "class D : C {}"
        };

        var result = await this.ProcessCompilationChangesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( result.InvalidatedSyntaxTrees, dependentFile );
        Assert.Same( result.DependenciesBefore.DependenciesByCompilation, result.DependenciesAfter.DependenciesByCompilation );
    }

    [Fact]
    public async Task PartialTypeDependency_PartialTypeAddedToExistingFile()
    {
        const string masterFile1 = "master1.cs";
        const string masterFile2 = "master2.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile1] = "partial class C {}", [masterFile2] = "", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.PartialType, "C", dependentFile ) };

        var codeAfter = new Dictionary<string, string>
        {
            [masterFile1] = "partial class C {}", [masterFile2] = "partial class C {}", [dependentFile] = "class D : C {}"
        };

        var result = await this.ProcessCompilationChangesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( result.InvalidatedSyntaxTrees, dependentFile );
        Assert.Same( result.DependenciesBefore.DependenciesByCompilation, result.DependenciesAfter.DependenciesByCompilation );
    }
}