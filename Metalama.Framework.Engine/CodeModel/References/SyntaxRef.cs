// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SyntaxRef<T> : CompilationBoundRef<T>
    where T : class, ICompilationElement
{
    public SyntaxNode SyntaxNode { get; }

    public SyntaxRef( SyntaxNode syntaxNode, RefTargetKind targetKind, CompilationContext compilationContext )
    {
        this.SyntaxNode = syntaxNode.AssertNotNull();
        this.TargetKind = targetKind;
        this.CompilationContext = compilationContext;
    }

    public override CompilationContext CompilationContext { get; }

    public override bool IsDefinition => true;

    public override IRef Definition => this;

    public override RefTargetKind TargetKind { get; }

    public override string Name => throw new NotSupportedException();

    protected override ISymbol GetSymbolIgnoringRefKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
    {
        Invariant.Assert( this.CompilationContext == compilationContext );

        return this.Symbol;
    }

    [Memo]
    private ISymbol Symbol => this.GetSymbol();

    private ISymbol GetSymbol()
    {
        var semanticModel =
            this.CompilationContext.SemanticModelProvider.GetSemanticModel( this.SyntaxNode.SyntaxTree )
            ?? throw new AssertionFailedException( $"Cannot get a semantic model for '{this.SyntaxNode.SyntaxTree.FilePath}'." );

        return (this.SyntaxNode is LambdaExpressionSyntax
                   ? semanticModel.GetSymbolInfo( this.SyntaxNode ).Symbol
                   : semanticModel.GetDeclaredSymbol( this.SyntaxNode ))
               ?? throw new AssertionFailedException( $"Cannot get a symbol for {this.SyntaxNode.GetType().Name}." );
    }

    protected override T? Resolve(
        CompilationModel compilation,
        ReferenceResolutionOptions options,
        bool throwIfMissing,
        IGenericContext? genericContext )
    {
        return ConvertOrThrow(
            compilation.Factory.GetCompilationElement(
                    this.Symbol,
                    this.TargetKind,
                    genericContext )
                .AssertNotNull(),
            compilation );
    }

    protected override bool EqualsCore( IRef? other, RefComparison comparison, IEqualityComparer<ISymbol> symbolComparer )
    {
        if ( other is not SyntaxRef<T> nodeRef )
        {
            return false;
        }

        if ( this.TargetKind != nodeRef.TargetKind )
        {
            return false;
        }

        return comparison switch
        {
            RefComparison.Default or RefComparison.IncludeNullability => nodeRef.SyntaxNode == this.SyntaxNode,
            _ => symbolComparer.Equals( this.Symbol, nodeRef.Symbol )
        };
    }

    protected override int GetHashCodeCore( RefComparison comparison, IEqualityComparer<ISymbol> symbolComparer )
        => comparison switch
        {
            RefComparison.Structural or RefComparison.StructuralIncludeNullability => symbolComparer.GetHashCode( this.Symbol ),
            _ => this.SyntaxNode.GetHashCode()
        };

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.SyntaxNode.GetType().Name,
            _ => $"{this.SyntaxNode.GetType().Name}:{this.TargetKind}"
        };

    public override IRefImpl<TOut> As<TOut>() => this as IRefImpl<TOut> ?? new SyntaxRef<TOut>( this.SyntaxNode, this.TargetKind, this.CompilationContext );
}