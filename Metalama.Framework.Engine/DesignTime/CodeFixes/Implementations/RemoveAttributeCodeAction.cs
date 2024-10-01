// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes.Implementations;

internal sealed partial class RemoveAttributeCodeAction : ICodeAction
{
    public RemoveAttributeCodeAction( IDeclaration targetDeclaration, INamedType attributeType )
    {
        this.TargetDeclaration = targetDeclaration;
        this.AttributeType = attributeType;
    }

    private IDeclaration TargetDeclaration { get; }

    private INamedType AttributeType { get; }

    public async Task ExecuteAsync( CodeActionContext context )
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var attributeTypeSymbol = (ITypeSymbol?) this.AttributeType.GetSymbol( context.CompilationContext )
                                  ??
                                  throw new InvalidOperationException(
                                      $"Cannot remove attributes of type '{this.AttributeType}' because the type does not exist in the source compilation." );

        var targetSymbol = this.TargetDeclaration.GetSymbol( context.CompilationContext )
                           ??
                           throw new InvalidOperationException(
                               $"Cannot remove attributes from '{this.TargetDeclaration}' because it does not exist in the source compilation." );

        // We need to process all syntaxes that define this symbol.
        foreach ( var syntaxReferenceGroup in targetSymbol.DeclaringSyntaxReferences.GroupBy( r => r.SyntaxTree ) )
        {
            var originalTree = syntaxReferenceGroup.Key;
            var originalRoot = await originalTree.GetRootAsync( context.CancellationToken );

            var rewriter = new RemoveAttributeRewriter(
                context.CompilationContext.SemanticModelProvider.GetSemanticModel( originalTree ),
                attributeTypeSymbol );

            var transformedRoot = originalRoot;
            var syntaxNodes = new List<SyntaxNode>();

            foreach ( var syntaxReference in syntaxReferenceGroup )
            {
                var originalNode = await syntaxReference.GetSyntaxAsync( context.CancellationToken );
                syntaxNodes.Add( originalNode );
            }

            transformedRoot = transformedRoot.ReplaceNodes( syntaxNodes, ( node, _ ) => rewriter.Visit( node )! );

            context.UpdateTree( transformedRoot, originalTree );
        }
    }
}