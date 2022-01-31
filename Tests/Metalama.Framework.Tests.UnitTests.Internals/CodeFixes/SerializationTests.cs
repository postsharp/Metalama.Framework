// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using MessagePack;
using MessagePack.Resolvers;
using Metalama.Framework.DesignTime.CodeFixes;
using Metalama.Framework.Engine.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;
using System.Collections.Immutable;
using System.IO;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeFixes;

public class SerializationTests
{
    private static T Roundloop<T>( T input )
    {

        var stringWriter = new StringWriter();
        var serializer = JsonSerializer.Create( new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
        serializer.Serialize( stringWriter, input);
        return serializer.Deserialize<T>( new JsonTextReader( new StringReader( stringWriter.ToString() ) ))!;
    }

    [Fact]
    public void Serialize_AddAspectAttributeCodeActionModel()
    {
        var roundloop = (AddAspectAttributeCodeActionModel) Roundloop(
            (CodeActionBaseModel)
            new AddAspectAttributeCodeActionModel( "AspectTypeName", "SymbolId", "SyntaxTreeFilePath" ) );

        Assert.Equal( "AspectTypeName", roundloop.AspectTypeName );
        Assert.Equal( "SymbolId", roundloop.TargetSymbolId );
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
            ImmutableArray.Create<CodeActionBaseModel>( new AddAspectAttributeCodeActionModel( "AspectTypeName", "SymbolId", "SyntaxTreeFilePath" ) ) );

        Roundloop( result );
    }

    [Fact]
    public void Serialize_CodeActionMenu()
    {
        var input = new CodeActionMenuModel( "The title" );
        input.Items.Add( new AddAspectAttributeCodeActionModel( "AspectTypeName", "SymbolId", "SyntaxTreeFilePath" ));

        var roundloop = (CodeActionMenuModel) Roundloop( (CodeActionBaseModel) input );
        
        Assert.Equal( "The title", roundloop.Title );
        Assert.Single( roundloop.Items );
    }

    [Fact]
    public void Serialize_CodeActionResult()
    {
        var code = "class Program { static void Main() {} }";
        var input = new CodeActionResult( new[] { CSharpSyntaxTree.ParseText( code, path: "path.cs" ), } );
        var roundloop = Roundloop( input );
        Assert.Single( roundloop.SyntaxTreeChanges );
        Assert.Equal( "path.cs", roundloop.SyntaxTreeChanges[0].FilePath );
        Assert.Equal( code, roundloop.SyntaxTreeChanges[0].SourceText );

    }
}