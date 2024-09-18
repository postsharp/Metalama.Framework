// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class SyntaxNodeRef<T> : BaseRef<T>
    where T : class, ICompilationElement
{
    public SyntaxNode SyntaxNode { get; }

    public SyntaxNodeRef( SyntaxNode syntaxNode, DeclarationRefTargetKind targetKind, CompilationContext compilationContext )
    {
        this.SyntaxNode = syntaxNode.AssertNotNull();
        this.TargetKind = targetKind;
        this.CompilationContext = compilationContext;
    }

    private protected override CompilationContext CompilationContext { get; }

    public override DeclarationRefTargetKind TargetKind { get; }

    public override string Name => throw new NotSupportedException();

    protected override ISymbol GetSymbolIgnoringKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
    {
        return GetSymbolOfNode( compilationContext, this.SyntaxNode );
    }

    private static ISymbol GetSymbolOfNode( CompilationContext compilationContext, SyntaxNode node )
    {
        var semanticModel =
            compilationContext.SemanticModelProvider.GetSemanticModel( node.SyntaxTree )
            ?? throw new AssertionFailedException( $"Cannot get a semantic model for '{node.SyntaxTree.FilePath}'." );

        var symbol =
            (node is LambdaExpressionSyntax ? semanticModel.GetSymbolInfo( node ).Symbol : semanticModel.GetDeclaredSymbol( node ))
            ?? throw new AssertionFailedException( $"Cannot get a symbol for {node.GetType().Name}." );

        return symbol;
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        return ConvertOrThrow(
            compilation.Factory.GetCompilationElement(
                    GetSymbolOfNode( compilation.PartialCompilation.CompilationContext, this.SyntaxNode ),
                    this.TargetKind )
                .AssertNotNull(),
            compilation );
    }

    public override bool Equals( IRef? other )
        => other is SyntaxNodeRef<T> nodeRef && nodeRef.SyntaxNode == this.SyntaxNode && nodeRef.TargetKind == this.TargetKind;

    protected override int GetHashCodeCore() => HashCode.Combine( this.SyntaxNode, this.TargetKind );

    public override string ToString()
        => this.TargetKind switch
        {
            DeclarationRefTargetKind.Default => this.SyntaxNode.GetType().Name,
            _ => $"{this.SyntaxNode.GetType().Name}:{this.TargetKind}"
        };
}