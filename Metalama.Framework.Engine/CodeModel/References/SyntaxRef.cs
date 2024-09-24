﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

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

    public override ICompilationBoundRefImpl WithGenericContext( GenericContext genericContext ) => throw new NotImplementedException();

    public override IRefCollectionStrategy CollectionStrategy => throw new NotSupportedException();

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

    public override RefComparisonKey GetComparisonKey() => new( this.Symbol );

    public override string ToString()
        => this.TargetKind switch
        {
            RefTargetKind.Default => this.SyntaxNode.GetType().Name,
            _ => $"{this.SyntaxNode.GetType().Name}:{this.TargetKind}"
        };

    public override IRefImpl<TOut> As<TOut>() => this as IRefImpl<TOut> ?? new SyntaxRef<TOut>( this.SyntaxNode, this.TargetKind, this.CompilationContext );
}