// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeFixes;

public class SerializationTests
{
    private static T Roundloop<T>( T input )
    {
        var stringWriter = new StringWriter();
        var serializer = JsonSerializer.Create( new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All } );
        serializer.Serialize( stringWriter, input );

        return serializer.Deserialize<T>( new JsonTextReader( new StringReader( stringWriter.ToString() ) ) )!;
    }

    [Fact]
    public void Serialize_AddAspectAttributeCodeActionModel()
    {
        var roundloop = (AddAspectAttributeCodeActionModel) Roundloop(
            (CodeActionBaseModel)
            new AddAspectAttributeCodeActionModel( "AspectTypeName", new SymbolId( "SymbolId" ), "SyntaxTreeFilePath" ) );

        Assert.Equal( "AspectTypeName", roundloop.AspectTypeName );
        Assert.Equal( "SymbolId", roundloop.TargetSymbolId.Id );
        Assert.Equal( "SyntaxTreeFilePath", roundloop.SyntaxTreeFilePath );
    }

    [Fact]
    public void Serialize_ImmutableArray()
    {
        var a = Roundloop( ImmutableArray.Create( 1, 2, 3 ) );
        Assert.Equal( 3, a.Length );
        Assert.Equal( 1, a[0] );
    }

    [Fact]
    public void Serialize_ComputeRefactoringResult()
    {
        var result = new ComputeRefactoringResult(
            ImmutableArray.Create<CodeActionBaseModel>(
                new AddAspectAttributeCodeActionModel( "AspectTypeName", new SymbolId( "SymbolId" ), "SyntaxTreeFilePath" ) ) );

        Roundloop( result );
    }

    [Fact]
    public void Serialize_CodeActionMenu()
    {
        var input = new CodeActionMenuModel( "The title" );
        input.Items.Add( new AddAspectAttributeCodeActionModel( "AspectTypeName", new SymbolId( "SymbolId" ), "SyntaxTreeFilePath" ) );

        var roundloop = (CodeActionMenuModel) Roundloop( (CodeActionBaseModel) input );

        Assert.Equal( "The title", roundloop.Title );
        Assert.Single( roundloop.Items );
    }

    [Fact]
    public void Serialize_CodeActionResult()
    {
        var code = "class Program { static void Main() {} }";
        var input = new CodeActionResult( new[] { CSharpSyntaxTree.ParseText( code, path: "path.cs" ) } );
        var roundloop = Roundloop( input );
        Assert.Single( roundloop.SyntaxTreeChanges );
        Assert.Equal( "path.cs", roundloop.SyntaxTreeChanges[0].FilePath );
        Assert.Equal( code, roundloop.SyntaxTreeChanges[0].SourceText );
    }

    [Fact]
    public void Serialize_SyntaxTree()
    {
        var code = "class Program { static void Main() {} }";
        var tree = CSharpSyntaxTree.ParseText( code, path: "path.cs" );
        var root = tree.GetRoot();
        var node = root.DescendantNodes().Single( n => n.IsKind( SyntaxKind.ClassDeclaration ) );
        var rootWithAnnotation = root.ReplaceNode( node, node.WithAdditionalAnnotations( Formatter.Annotation ) );
        var treeWithAnnotation = tree.WithRootAndOptions( rootWithAnnotation, tree.Options );
        var input = new SerializableSyntaxTree( treeWithAnnotation );
        var roundloop = Roundloop( input );
        Assert.Single( roundloop.Annotations );
        Assert.Equal( SerializableAnnotationKind.Formatter, roundloop.Annotations[0].Kind );
        Assert.Equal( node.Span, roundloop.Annotations[0].TextSpan );

        var roundloopRoot = roundloop.GetAnnotatedSyntaxNode();
        var roundloopNode = roundloopRoot.DescendantNodes().Single( n => n.IsKind( SyntaxKind.ClassDeclaration ) );
        Assert.True( roundloopNode.HasAnnotation( Formatter.Annotation ) );
    }
}