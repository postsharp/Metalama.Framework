// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Refactoring;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes;

public class AddAspectAttributeCodeActionModel : CodeActionModel
{
    public string AspectTypeName { get; set; }

    public string TargetSymbolId { get; set; }

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

    public override async Task<CodeActionResult> ExecuteAsync( CodeActionExecutionContext executionContext, CancellationToken cancellationToken )
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

        var targetSymbol = new SymbolId( this.TargetSymbolId ).Resolve( compilation.RoslynCompilation, cancellationToken: cancellationToken );

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

        return new CodeActionResult( ImmutableArray.Create( new SerializableSyntaxTree( syntaxTree.FilePath, newSyntaxRoot ) ) );
    }
}