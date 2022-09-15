// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    private async Task<List<string>> GetInvalidatedSyntaxTreesAsync(
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

        var dependencyGraph = DependencyGraph.Create( compilationVersion1, dependencyCollector );

        var compilation2 = CreateCSharpCompilation( codeAfter, name: "test", ignoreErrors: true );

        var changes = await compilationChangesProvider.GetCompilationChangesAsync( compilation1, compilation2 );
        var invalidatedSyntaxTrees = new HashSet<string>();

        await compilationChangesProvider.InvokeForInvalidatedSyntaxTreesAsync( changes, dependencyGraph, t => invalidatedSyntaxTrees.Add( t ) );

        return invalidatedSyntaxTrees.OrderBy( x => x ).ToList();
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

        var invalidated = await this.GetInvalidatedSyntaxTreesAsync( codeBefore, dependencies, codeAfter );

        Assert.Empty( invalidated );
    }

    [Fact]
    public async Task SyntaxTreeDependency_FileChanged()
    {
        const string masterFile = "master.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile] = "class C {}", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.SyntaxTree, masterFile, dependentFile ) };
        var codeAfter = new Dictionary<string, string> { [masterFile] = "class C { void M() {} }", [dependentFile] = "class D : C {}" };
     
        var invalidated = await this.GetInvalidatedSyntaxTreesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( invalidated, dependentFile );
    }

    [Fact]
    public async Task SyntaxTreeDependency_FileRemoved()
    {
        const string masterFile = "master.cs";
        const string dependentFile = "dependent.cs";
        var codeBefore = new Dictionary<string, string> { [masterFile] = "class C {}", [dependentFile] = "class D : C {}" };
        var dependencies = new[] { new Dependency( DependencyKind.SyntaxTree, masterFile, dependentFile ) };
        var codeAfter = new Dictionary<string, string> { [dependentFile] = "class D : C {}" };
     
        var invalidated = await this.GetInvalidatedSyntaxTreesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( invalidated, dependentFile );
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

        var invalidated = await this.GetInvalidatedSyntaxTreesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( invalidated, dependentFile );
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

        var invalidated = await this.GetInvalidatedSyntaxTreesAsync( codeBefore, dependencies, codeAfter );

        Assert.Single( invalidated, dependentFile );
    }
}