// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public partial class CompilationChangeTrackerTests
{
    [Fact]
    public void AddSyntaxTree_Standard()
    {
        var code = new Dictionary<string, string>();
        var compilation1 = CreateCSharpCompilation( code );

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code.Add( "code.cs", "class C { }" );

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.False( changes.HasCompileTimeCodeChange );

        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single();

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

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code.Add( "code.cs", "using Metalama.Framework.Aspects; class C { }" );

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.True( changes.HasCompileTimeCodeChange );

        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single();

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

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code.Add( "code.cs", "partial class C { }" );

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.False( changes.HasCompileTimeCodeChange );

        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single();

        Assert.Equal( SyntaxTreeChangeKind.Added, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.None, syntaxTreeChange.CompileTimeChangeKind );
        Assert.Single( syntaxTreeChange.AddedPartialTypes );
        Assert.Single( changes.AddedPartialTypes );
    }

    [Fact]
    public void RemoveSyntaxTree()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C { }" } };
        var compilation1 = CreateCSharpCompilation( code );

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code.Clear();

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.False( changes.HasCompileTimeCodeChange );

        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single();

        Assert.Equal( SyntaxTreeChangeKind.Deleted, syntaxTreeChange.SyntaxTreeChangeKind );
        Assert.Equal( CompileTimeChangeKind.None, syntaxTreeChange.CompileTimeChangeKind );
        Assert.True( syntaxTreeChange.NewSyntaxTreeVersion.IsDefault );
    }

    [Fact]
    public void ChangeSyntaxTree_ChangeDeclaration()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code["code.cs"] = "class D {}";

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.False( changes.HasCompileTimeCodeChange );

        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.CompilationToAnalyze );
        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single();

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

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code["code.cs"] = "partial class C { void N() {} }";

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.False( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.Empty( changes.AddedPartialTypes );
        Assert.True( changes.IsIncremental );
        Assert.Equal( compilation2, changes.CompilationToAnalyze );

        Assert.Single( changes.SyntaxTreeChanges );

        var syntaxTreeChange = changes.SyntaxTreeChanges.Single();

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

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code["code.cs"] = "class C { int M => 2; }";

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );

        Assert.False( changes.HasChange );
        Assert.False( changes.HasCompileTimeCodeChange );
    }

    [Fact]
    public void ChangeSyntaxTree_AddPartialType()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code["code.cs"] = "class C {} partial class D {}";

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.False( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.Single( changes.AddedPartialTypes );
        Assert.True( changes.IsIncremental );
        Assert.Single( changes.SyntaxTreeChanges );
    }

    [Fact]
    public void ChangeSyntaxTree_NewlyCompileTime()
    {
        var code = new Dictionary<string, string> { { "code.cs", "class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code["code.cs"] = "using Metalama.Framework.Aspects; class C {}";

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Single( changes.SyntaxTreeChanges );
        Assert.Equal( CompileTimeChangeKind.NewlyCompileTime, changes.SyntaxTreeChanges.Single().CompileTimeChangeKind );
    }

    [Fact]
    public void ChangeSyntaxTree_NoLongerCompileTime()
    {
        var code = new Dictionary<string, string> { { "code.cs", "using Metalama.Framework.Aspects; class C {}" } };
        var compilation1 = CreateCSharpCompilation( code );

        var tracker = new CompilationVersion( this._strategy )
            .Update( compilation1 )
            .ResetChanges();

        Assert.False( tracker.Changes!.HasChange );

        code["code.cs"] = "class C {}";

        var compilation2 = CreateCSharpCompilation( code );
        tracker = tracker.Update( compilation2 );
        var changes = tracker.Changes!;
        Assert.NotNull( changes );
        Assert.True( changes.HasCompileTimeCodeChange );
        Assert.True( changes.HasChange );
        Assert.True( changes.IsIncremental );
        Assert.Single( changes.SyntaxTreeChanges );
        Assert.Equal( CompileTimeChangeKind.NoLongerCompileTime, changes.SyntaxTreeChanges.Single().CompileTimeChangeKind );
    }
}