// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Refactoring;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Immutable;
using System.Runtime.Serialization;

namespace Metalama.Framework.DesignTime.CodeFixes;

[DataContract]
public class AddAspectAttributeCodeActionModel : CodeActionModel
{
    [DataMember( Order = NextKey + 0 )]
    public string AspectTypeName { get; set; }

    [DataMember( Order = NextKey + 1 )]
    public string TargetSymbolId { get; set; }

    [DataMember( Order = NextKey + 2 )]
    public string SyntaxTreeFilePath { get; set; }

    public AddAspectAttributeCodeActionModel( string aspectTypeName, string targetSymbolId, string syntaxTreeFilePath ) : base(
        $"Add [{AttributeHelper.GetShortName( aspectTypeName )}]" )
    {
        this.AspectTypeName = aspectTypeName;
        this.TargetSymbolId = targetSymbolId;
        this.SyntaxTreeFilePath = syntaxTreeFilePath;
    }

    // Deserializing constructor.
    public AddAspectAttributeCodeActionModel()
    {
        this.AspectTypeName = null!;
        this.TargetSymbolId = null!;
        this.SyntaxTreeFilePath = null!;
    }

    protected override async Task<CodeActionResult> ExecuteAsync( CodeActionExecutionContext executionContext, CancellationToken cancellationToken )
    {
        var lastDot = this.AspectTypeName.LastIndexOf( '.' );
        string ns, typeName;

        if ( lastDot >= 0 )
        {
            ns = this.AspectTypeName.Substring( 0, lastDot - 1 );
            typeName = this.AspectTypeName.Substring( lastDot + 1 );
        }
        else
        {
            ns = "";
            typeName = this.AspectTypeName;
        }

        var attributeDescription = new AttributeDescription(
            AttributeHelper.GetShortName( typeName ),
            imports: ImmutableList.Create( ns ) );

        var compilation = executionContext.Compilation;

        // Find the syntax tree.
        if ( !compilation.PartialCompilation.SyntaxTrees.TryGetValue( this.SyntaxTreeFilePath, out var syntaxTree ) )
        {
            executionContext.Logger.Warning?.Log( "Cannot find the syntax tree." );

            return CodeActionResult.Empty;
        }

        var syntaxRoot = await syntaxTree.GetRootAsync( cancellationToken );

        var targetSymbol = compilation.Factory.GetDeclarationFromId( this.TargetSymbolId )?.GetSymbol();

        if ( targetSymbol == null )
        {
            executionContext.Logger.Warning?.Log( "Cannot find the symbol." );

            return CodeActionResult.Empty;
        }

        var oldNode =
            await targetSymbol.DeclaringSyntaxReferences.SingleOrDefault( r => r.SyntaxTree == syntaxRoot.SyntaxTree )!.GetSyntaxAsync( cancellationToken );

        var newSyntaxRoot = await CSharpAttributeHelper.AddAttributeAsync( syntaxRoot, oldNode, attributeDescription, cancellationToken );

        if ( newSyntaxRoot == null )
        {
            return CodeActionResult.Empty;
        }

        return new CodeActionResult( ImmutableArray.Create( new SyntaxTreeChange( syntaxTree.FilePath, newSyntaxRoot ) ) );
    }
}