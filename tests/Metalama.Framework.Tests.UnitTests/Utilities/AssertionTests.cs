// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Utilities;

#pragma warning disable VSTHRD200

public sealed class AssertionTests : UnitTestClass
{
    [Fact]
    public void Symbols()
    {
        var code = @"
using System;

public class X
{
    public int A;
    public int B { get; set; }
    public event EventHandler C;
    public void D(int p) { }
    public static X() { }
    public X() { }
    public ~X() { }
}
";

        var compilation = CSharpCompilation.Create( null, [CSharpSyntaxTree.ParseText( code )] );

        var typeX = compilation.Assembly.GlobalNamespace.GetTypeMembers().Single();
        var memberA = typeX.GetMembers().Single( m => m.Name == "A" );
        var memberB = typeX.GetMembers().Single( m => m.Name == "B" );
        var memberC = typeX.GetMembers().Single( m => m.Name == "C" );
        var memberD = typeX.GetMembers().Single( m => m.Name == "D" );
        var memberDP = ((IMethodSymbol)typeX.GetMembers().Single( m => m.Name == "D" )).Parameters[0];
        var memberX = typeX.GetMembers().Single( m => m is IMethodSymbol { MethodKind: MethodKind.Constructor } );
        var memberTildeX = typeX.GetMembers().Single( m => m is IMethodSymbol { MethodKind: MethodKind.Destructor } );

        Assert.Equal( "NamedType:X:[SourceFile([32..33))]", new AssertionFailedException( $"{typeX}" ).Message );
        Assert.Equal( "Field:X.A:[SourceFile([53..54))]", new AssertionFailedException( $"{memberA}" ).Message );
        Assert.Equal( "Property:X.B:[SourceFile([72..73))]", new AssertionFailedException( $"{memberB}" ).Message );
        Assert.Equal( "Event:X.C:[SourceFile([119..120))]", new AssertionFailedException( $"{memberC}" ).Message );
        Assert.Equal( "Method:X.D(int):[SourceFile([139..140))]", new AssertionFailedException( $"{memberD}" ).Message );
        Assert.Equal( "Parameter:X.D(int):int p:[SourceFile([145..146))]", new AssertionFailedException( $"{memberDP}" ).Message );
        Assert.Equal( "Method:X.X():[SourceFile([191..192))]", new AssertionFailedException( $"{memberX}" ).Message );
        Assert.Equal( "Method:X.~X():[SourceFile([212..213))]", new AssertionFailedException( $"{memberTildeX}" ).Message );
    }

    [Fact]
    public void Errors()
    {
        var code = @"
using System;

public class X
{
    private Goo z;

    public void Foo()
    {
        _ = this.Quz(); // Non-existent member.
        _ = new NS.X(); // Non-existent namespace.
        _ = Y.Bar(); // Inaccessible member.
    }
}

public class Y
{
    static int Bar() => 42;
}
";

        var compilation = CSharpCompilation.Create( null, [CSharpSyntaxTree.ParseText( code )] );

        // Just make sure we are able to create exception for all symbols.
        var symbols =
            compilation.SyntaxTrees
            .SelectMany( s => s.GetRoot().DescendantNodesAndSelf() )
            .Select( n => compilation.GetSemanticModel( n.SyntaxTree ).GetSymbolInfo( n ) )
            .SelectMany( si => si.CandidateSymbols.Append( si.Symbol ) )
            .Select( s => new AssertionFailedException( $"{s}" ) )
            .ToList();
    }
}