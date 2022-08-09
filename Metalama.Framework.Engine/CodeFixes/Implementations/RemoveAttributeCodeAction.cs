// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes.Implementations;

internal partial class RemoveAttributeCodeAction : ICodeAction
{
    public RemoveAttributeCodeAction( IDeclaration targetDeclaration, INamedType attributeType )
    {
        this.TargetDeclaration = targetDeclaration;
        this.AttributeType = attributeType;
    }

    public IDeclaration TargetDeclaration { get; }

    public INamedType AttributeType { get; }

    public async Task ExecuteAsync( CodeActionContext context )
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var compilation = context.Compilation.Compilation;

        var attributeTypeSymbol = (ITypeSymbol?) this.AttributeType.GetSymbol( compilation );

        if ( attributeTypeSymbol == null )
        {
            throw new InvalidOperationException(
                $"Cannot remove attributes of type '{this.AttributeType}' because the type does not exist in the source compilation." );
        }

        var targetSymbol = this.TargetDeclaration.GetSymbol( compilation );

        if ( targetSymbol == null )
        {
            throw new InvalidOperationException(
                $"Cannot remove attributes from '{this.TargetDeclaration}' because it does not exist in the source compilation." );
        }

        // We need to process all syntaxes that define this symbol.
        foreach ( var syntaxReferenceGroup in targetSymbol.DeclaringSyntaxReferences.GroupBy( r => r.SyntaxTree ) )
        {
            var originalTree = syntaxReferenceGroup.Key;
            var originalRoot = await originalTree.GetRootAsync( context.CancellationToken );

            var rewriter = new RemoveAttributeRewriter( compilation.GetSemanticModel( originalTree ), attributeTypeSymbol );

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