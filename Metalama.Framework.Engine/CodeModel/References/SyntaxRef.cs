// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class SyntaxRef<T> : CompilationBoundRef<T>
    where T : class, ICompilationElement
{
    private ISymbol? _cachedSymbol;

    public SyntaxNode SyntaxNode { get; }

    public SyntaxRef( SyntaxNode syntaxNode, RefTargetKind targetKind, CompilationContext compilationContext )
    {
        this.SyntaxNode = syntaxNode.AssertNotNull();
        this.TargetKind = targetKind;
        this.CompilationContext = compilationContext;
    }

    public override CompilationContext CompilationContext { get; }

    public override RefTargetKind TargetKind { get; }

    public override string Name => throw new NotSupportedException();

    protected override ISymbol GetSymbolIgnoringKind( bool ignoreAssemblyKey = false )
    {
        return this.GetSymbol();
    }

    private ISymbol GetSymbol()
    {
        if ( this._cachedSymbol != null )
        {
            return this._cachedSymbol;
        }

        var semanticModel =
            this.CompilationContext.SemanticModelProvider.GetSemanticModel( this.SyntaxNode.SyntaxTree )
            ?? throw new AssertionFailedException( $"Cannot get a semantic model for '{this.SyntaxNode.SyntaxTree.FilePath}'." );

        this._cachedSymbol =
            (this.SyntaxNode is LambdaExpressionSyntax
                ? semanticModel.GetSymbolInfo( this.SyntaxNode ).Symbol
                : semanticModel.GetDeclaredSymbol( this.SyntaxNode ))
            ?? throw new AssertionFailedException( $"Cannot get a symbol for {this.SyntaxNode.GetType().Name}." );

        return this._cachedSymbol;
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        Invariant.Assert( compilation.GetCompilationContext() == this.CompilationContext, "CompilationContext mismatch." );

        return ConvertOrThrow(
            compilation.Factory.GetCompilationElement(
                    this.GetSymbol(),
                    this.TargetKind )
                .AssertNotNull(),
            compilation );
    }

    public override bool Equals( IRef? other, RefComparisonOptions options )
    {
        if ( other is not SyntaxRef<T> nodeRef )
        {
            if ( (options & RefComparisonOptions.Structural) != 0 )
            {
                throw new NotImplementedException( "Structural comparisons of different reference kinds is not implemented." );
            }
            else
            {
                return false;
            }
        }

        Invariant.Assert( this.CompilationContext == nodeRef.CompilationContext, "Attempted to compare two symbols of different compilations." );

        if ( this.TargetKind != nodeRef.TargetKind )
        {
            return false;
        }

        return options switch
        {
            RefComparisonOptions.Default or RefComparisonOptions.IncludeNullability => nodeRef.SyntaxNode == this.SyntaxNode,
            _ => options.GetSymbolComparer().Equals( this.GetSymbol(), nodeRef.GetSymbol() )
        };
    }

    public override int GetHashCode( RefComparisonOptions comparisonOptions )
        => HashCode.Combine(
            comparisonOptions switch
            {
                RefComparisonOptions.Default or RefComparisonOptions.IncludeNullability => this.SyntaxNode.GetHashCode(),
                _ => comparisonOptions.GetSymbolComparer().GetHashCode( this.GetSymbol() )
            },
            this.TargetKind );

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.SyntaxNode.GetType().Name,
            _ => $"{this.SyntaxNode.GetType().Name}:{this.TargetKind}"
        };

    public override IRefImpl<TOut> As<TOut>() => this as IRefImpl<TOut> ?? new SyntaxRef<TOut>( this.SyntaxNode, this.TargetKind, this.CompilationContext );
}