// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed class BuilderRef<T> : CompilationBoundRef<T>, IBuilderRef
    where T : class, IDeclaration
{
    public BuilderRef( IDeclarationBuilder builder, GenericContext? genericContext, CompilationContext compilationContext )
    {
        Invariant.Assert( typeof(T) == builder.DeclarationKind.GetRefInterfaceType() );

        this.Builder = builder;
        this.GenericContext = genericContext ?? GenericContext.Empty;
        this.CompilationContext = compilationContext;
    }

    public override CompilationContext CompilationContext { get; }

    public override bool IsDefinition => this.GenericContext.IsEmptyOrIdentity;

    public override IRef Definition => this.IsDefinition ? this : this.Builder.ToRef();

    public IDeclarationBuilder Builder { get; }

    public GenericContext GenericContext { get; } // Gives the type arguments for the builder.

    public override IRefStrategy Strategy => BuilderRefStrategy.Instance;

    public override string Name
        => this.Builder switch
        {
            INamedDeclaration named => named.Name,
            _ => throw new NotSupportedException( $"Declarations of kind {this.Builder.DeclarationKind} have no name." )
        };

    public override SerializableDeclarationId ToSerializableId() => this.Builder.ToSerializableId();

    protected override ISymbol GetSymbolIgnoringKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
        => throw new NotSupportedException();

    public override ISymbol GetClosestContainingSymbol( CompilationContext compilationContext )
    {
        Invariant.Assert( compilationContext == this.CompilationContext );

        for ( var ancestor = this.Builder.ContainingDeclaration; ancestor != null; ancestor = ancestor.ContainingDeclaration )
        {
            if ( ancestor is SymbolBasedDeclaration symbolBasedDeclaration )
            {
                return symbolBasedDeclaration.Symbol;
            }
        }

        // We should always have an containing symbol.
        throw new AssertionFailedException();
    }

    // SMELL
    private GenericContext GetCombinedGenericMap( IGenericContext? genericContext )
    {
        if ( this.GenericContext.IsEmptyOrIdentity )
        {
            return (GenericContext?) genericContext ?? GenericContext.Empty;
        }
        else if ( genericContext is null or { IsEmptyOrIdentity: true } )
        {
            return this.GenericContext;
        }
        else
        {
            throw new InvalidOperationException( "Cannot combine two non-empty generic contexts." );
        }
    }

    protected override T? Resolve(
        CompilationModel compilation,
        ReferenceResolutionOptions options,
        bool throwIfMissing,
        IGenericContext? genericContext )
    {
        return ConvertOrThrow(
            compilation.Factory.GetDeclaration( this.Builder, options, this.GetCombinedGenericMap( genericContext ), throwIfMissing ),
            compilation );
    }

    protected override bool EqualsCore( IRef? other, RefComparison options, IEqualityComparer<ISymbol> symbolComparer )
    {
        if ( other is not BuilderRef<T> builderRef )
        {
            return false;
        }

        return this.Builder == builderRef.Builder;
    }

    protected override int GetHashCodeCore( RefComparison comparison, IEqualityComparer<ISymbol> symbolComparer ) => this.Builder.GetHashCode();

    public override string ToString() => this.Builder.ToString()!;

    public override IRefImpl<TOut> As<TOut>()
        => (IRefImpl<TOut>) (object) this; // There should be no reason to upcast since we always create instances of the right type.
}