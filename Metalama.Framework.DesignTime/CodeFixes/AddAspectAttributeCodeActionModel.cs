// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Refactoring;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// Represents a code action that adds a custom attribute representing an aspect.
/// </summary>
internal class AddAspectAttributeCodeActionModel : CodeActionModel
{
    /// <summary>
    /// Gets or sets the full type of the aspect to add.
    /// </summary>
    public string AspectTypeName { get; set; }

    /// <summary>
    /// Gets or sets id of the symbol to which the custom attribute should be added.
    /// </summary>
    public SymbolId TargetSymbolId { get; set; }

    /// <summary>
    /// Gets or sets the path of the file in which the custom attribute should be added.
    /// </summary>
    public string SyntaxTreeFilePath { get; set; }

    public AddAspectAttributeCodeActionModel(
        string aspectTypeName,
        SymbolId targetSymbolId,
        string syntaxTreeFilePath ) : base( $"Add [{AttributeHelper.GetShortName( aspectTypeName )}]" )
    {
        this.AspectTypeName = aspectTypeName;
        this.TargetSymbolId = targetSymbolId;
        this.SyntaxTreeFilePath = syntaxTreeFilePath;
    }

    // Deserializing constructor.
    public AddAspectAttributeCodeActionModel()
    {
        this.AspectTypeName = null!;
        this.SyntaxTreeFilePath = null!;
    }

    public override async Task<CodeActionResult> ExecuteAsync(
        CodeActionExecutionContext executionContext,
        bool isComputingPreview,
        TestableCancellationToken cancellationToken )
    {
        AttributeHelper.Parse( this.AspectTypeName, out var ns, out _, out var shortName );

        var attributeDescription = new AttributeDescription( shortName, imports: ImmutableList.Create( ns ) );

        var compilation = executionContext.Compilation;

        // Find the syntax tree.
        if ( !compilation.PartialCompilation.SyntaxTrees.TryGetValue( this.SyntaxTreeFilePath, out var syntaxTree ) )
        {
            executionContext.Logger.Warning?.Log( "Cannot find the syntax tree." );

            return CodeActionResult.Empty;
        }

        var syntaxRoot = await syntaxTree.GetRootAsync( cancellationToken );

        var targetSymbol = this.TargetSymbolId.Resolve( compilation.RoslynCompilation, cancellationToken: cancellationToken );

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

        return CodeActionResult.Success( ImmutableArray.Create( new SerializableSyntaxTree( syntaxTree.FilePath, newSyntaxRoot ) ) );
    }
}