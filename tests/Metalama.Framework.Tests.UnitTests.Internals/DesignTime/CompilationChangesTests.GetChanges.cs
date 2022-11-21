﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.DesignTime.Rpc;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public partial class CompilationChangesTests
{
    private CompilationChanges CompareSyntaxTrees( Compilation compilation1, Compilation compilation2 )
        => CompilationChanges.Incremental(
            ProjectVersion.Create( compilation1, compilation1.GetProjectKey(), this._strategy ),
            compilation2,
            new ReferenceChanges(
                ImmutableDictionary<ProjectKey, IProjectVersion>.Empty,
                ImmutableDictionary<ProjectKey, ReferencedProjectChange>.Empty,
                ImmutableHashSet<string>.Empty,
                ImmutableDictionary<string, ReferencedPortableExecutableChange>.Empty ) );

    [Fact]
    public void AddSyntaxTree_Standard()
    {
        var code = new Dictionary<string, string>();
        var compilation1 = CreateCSharpCompilation( code );

        code.Add( "code.cs", "class C { }" );

        var compilation2 = CreateCSharpCompilation( code );
        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.False( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.NewProjectVersion.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single().Value;

        Assert.Equal( SyntaxTreeChangeKind.Added, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.None, syntaxTreeChange.CompileTimeChangeKind );
        Assert.True( syntaxTreeChange.OldSyntaxTreeVersion.IsDefault );
        Assert.NotNull( syntaxTreeChange.NewTree );
    }

    [Fact]
    public void AddSyntaxTree_CompileTime()
    {
        var code = new Dictionary<string, string>();
        var compilation1 = CreateCSharpCompilation( code );

        code.Add( "code.cs", "using Metalama.Framework.Aspects; class C { }" );

        var compilation2 = CreateCSharpCompilation( code );
        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.NewProjectVersion.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single().Value;

        Assert.Equal( SyntaxTreeChangeKind.Added, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.NewlyCompileTime, syntaxTreeChange.CompileTimeChangeKind );
        Assert.True( syntaxTreeChange.OldSyntaxTreeVersion.IsDefault );
        Assert.NotNull( syntaxTreeChange.NewTree );
    }

    [Fact]
    public void AddSyntaxTree_PartialType()
    {
        var code = new Dictionary<string, string>();
        var compilation1 = CreateCSharpCompilation( code );

        code.Add( "code.cs", "partial class C { }" );

        var compilation2 = CreateCSharpCompilation( code );
        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.False( changes.HasCompileTimeCodeChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.NewProjectVersion.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single().Value;

        Assert.Equal( SyntaxTreeChangeKind.Added, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.None, syntaxTreeChange.CompileTimeChangeKind );
        Assert.Single( syntaxTreeChange.PartialTypeChanges );
        Assert.Equal( PartialTypeChangeKind.Added, syntaxTreeChange.PartialTypeChanges[0].Kind );
    }

    [Fact]
    public void RemoveSyntaxTree()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C { }" } };
        var compilation1 = CreateCSharpCompilation( code );

        code.Clear();

        var compilation2 = CreateCSharpCompilation( code );

        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.False( changes.HasCompileTimeCodeChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.NewProjectVersion.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single().Value;

        Assert.Equal( SyntaxTreeChangeKind.Removed, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.None, syntaxTreeChange.CompileTimeChangeKind );
        Assert.True( syntaxTreeChange.NewSyntaxTreeVersion.IsDefault );
    }

    [Fact]
    public void ChangeSyntaxTree_ChangeDeclaration()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        code["code.cs"] = "class D {}";

        var compilation2 = CreateCSharpCompilation( code );
        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.True( changes.HasChange );
        Assert.False( changes.HasCompileTimeCodeChange );

        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.NewProjectVersion.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single().Value;

        Assert.Equal( SyntaxTreeChangeKind.Changed, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.None, syntaxTreeChange.CompileTimeChangeKind );
        Assert.Equal( compilation1.SyntaxTrees.Single(), syntaxTreeChange.OldSyntaxTreeVersion.SyntaxTree );
        Assert.Equal( compilation2.SyntaxTrees.Single(), syntaxTreeChange.NewSyntaxTreeVersion.SyntaxTree );
    }

    [Fact]
    public void ChangeSyntaxTree_ChangePartialTypeMembers()
    {
        var code = new Dictionary<string, string> { { "code.cs", "partial class C { void M() {} }" } };
        var compilation1 = CreateCSharpCompilation( code );

        code["code.cs"] = "partial class C { void N() {} }";

        var compilation2 = CreateCSharpCompilation( code );

        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.False( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.Single( changes.SyntaxTreeChanges );

        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.NewProjectVersion.CompilationToAnalyze );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single().Value;

        Assert.Empty( syntaxTreeChange.PartialTypeChanges );
        Assert.Equal( SyntaxTreeChangeKind.Changed, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.None, syntaxTreeChange.CompileTimeChangeKind );
        Assert.Equal( compilation1.SyntaxTrees.Single(), syntaxTreeChange.OldSyntaxTreeVersion.SyntaxTree );
        Assert.Equal( compilation2.SyntaxTrees.Single(), syntaxTreeChange.NewSyntaxTreeVersion.SyntaxTree );
    }

    [Fact]
    public void ChangeSyntaxTree_ChangeBody()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C { int M => 1; }" } };
        var compilation1 = CreateCSharpCompilation( code );

        code["code.cs"] = "class C { int M => 2; }";

        var compilation2 = CreateCSharpCompilation( code );

        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.False( changes.HasChange );
        Assert.False( changes.HasCompileTimeCodeChange );
    }

    [Fact]
    public void ChangeSyntaxTree_AddPartialType()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        code["code.cs"] = "class C {} partial class D {}";

        var compilation2 = CreateCSharpCompilation( code );

        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.False( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single().Value;
        Assert.Single( syntaxTreeChange.PartialTypeChanges );
    }

    [Fact]
    public void ChangeSyntaxTree_NewlyCompileTime()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        code["code.cs"] = "using Metalama.Framework.Aspects; class C {}";

        var compilation2 = CreateCSharpCompilation( code );

        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Single( changes.SyntaxTreeChanges );
        Assert.Equal( CompileTimeChangeKind.NewlyCompileTime, changes.SyntaxTreeChanges.Single().Value.CompileTimeChangeKind );
    }

    [Fact]
    public void ChangeSyntaxTree_NoLongerCompileTime()
    {
        var code = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        code["code.cs"] = "class C {}";

        var compilation2 = CreateCSharpCompilation( code );

        var changes = this.CompareSyntaxTrees( compilation1, compilation2 );

        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Single( changes.SyntaxTreeChanges );
        Assert.Equal( CompileTimeChangeKind.NoLongerCompileTime, changes.SyntaxTreeChanges.Single().Value.CompileTimeChangeKind );
    }
}