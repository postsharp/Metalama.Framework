// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed partial class SyntaxRef<T> : FullRef<T>
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

    public override FullRef<T> WithGenericContext( GenericContext genericContext ) => throw new NotImplementedException();

    public override bool IsDefinition => true;

    public override IFullRef<T> Definition => this;

    public override RefTargetKind TargetKind { get; }

    public override IFullRef? ContainingDeclaration => throw new NotImplementedException();

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
        bool throwIfMissing,
        IGenericContext? genericContext )
    {
        return ConvertDeclarationOrThrow(
            compilation.Factory.GetCompilationElement(
                    this.Symbol,
                    this.TargetKind,
                    genericContext )
                .AssertNotNull(),
            compilation );
    }

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.SyntaxNode.GetType().Name,
            _ => $"{this.SyntaxNode.GetType().Name}:{this.TargetKind}"
        };

    public override bool IsValid => true;

    protected override IFullRef<TOut> CastAsFullRef<TOut>()
        => this as IFullRef<TOut> ?? new SyntaxRef<TOut>( this.SyntaxNode, this.TargetKind, this.CompilationContext );

    public override bool Equals( IRef? other, RefComparison comparison )
    {
        if ( comparison != RefComparison.Default )
        {
            throw new ArgumentOutOfRangeException( nameof(comparison), "Only RefComparison.Default is supported for SyntaxRef." );
        }

        if ( other is null )
        {
            return false;
        }

        // The whole point of SyntaxRef is to avoid resolving the semantic model until and if necessary.
        // Therefore, by design, we don't resolve to symbols before comparing, which means that we cannot
        // compare to other kind of references.
        if ( other is not SyntaxRef<T> syntaxRef )
        {
            throw new NotSupportedException( "A SyntaxRef can only be compared to another SyntaxRef." );
        }

        return this.SyntaxNode == syntaxRef.SyntaxNode && this.TargetKind == syntaxRef.TargetKind;
    }

    public override int GetHashCode( RefComparison comparison )
    {
        if ( comparison != RefComparison.Default )
        {
            throw new ArgumentOutOfRangeException( nameof(comparison), "Only RefComparison.Default is supported for SyntaxRef." );
        }

        return HashCode.Combine( this.SyntaxNode, this.TargetKind );
    }

    public override DeclarationKind DeclarationKind => throw new NotSupportedException();
}