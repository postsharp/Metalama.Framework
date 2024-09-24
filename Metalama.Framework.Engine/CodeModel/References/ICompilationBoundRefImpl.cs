// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface ICompilationBoundRefImpl : IRefImpl
{
    CompilationContext CompilationContext { get; }

    ResolvedAttributeRef GetAttributeData();

    bool IsDefinition { get; }

    IRef Definition { get; }

    ICompilationBoundRefImpl WithGenericContext( GenericContext genericContext );

    IRefCollectionStrategy CollectionStrategy { get; }

    RefComparisonKey GetComparisonKey();
}

internal record struct RefComparisonKey
{
    // Intentionally a struct and not a class to reduce memory overhead during comparisons.

    public RefComparisonKey( ISymbol symbol )
    {
        this.SymbolOrBuilder = symbol;
        this.GenericContext = GenericContext.Empty;
    }

    public RefComparisonKey( IDeclarationBuilder builder, GenericContext GenericContext )
    {
        this.SymbolOrBuilder = builder;
        this.GenericContext = GenericContext;
    }

    public object SymbolOrBuilder { get; set; }

    public GenericContext GenericContext { get; set; }
}