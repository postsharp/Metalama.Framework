// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// An implementation of <see cref="IRef"/> based on <see cref="IDeclarationBuilder"/>.
/// </summary>
internal sealed class BuilderRef<T> : CompilationBoundRef<T>, IBuilderRef
    where T : class, IDeclaration
{
    public BuilderRef( IDeclarationBuilder builder, GenericContext? genericContext, CompilationContext compilationContext )
    {
        Invariant.Assert(
            builder.DeclarationKind.GetPossibleDeclarationInterfaceTypes().Contains( typeof(T) ),
            $"The interface type was expected to be of type {builder.DeclarationKind.GetPossibleDeclarationInterfaceTypes()} but was {typeof(T)}." );

        // Constructor replacements must be resolved upstream.
        Invariant.Assert( builder is not ConstructorBuilder { ReplacedImplicitConstructor: not null } );

        this.Builder = builder;
        this.GenericContext = genericContext ?? GenericContext.Empty;
        this.CompilationContext = compilationContext;
    }

    public override CompilationContext CompilationContext { get; }

    public override bool IsDefinition => this.GenericContext.IsEmptyOrIdentity;

    public override IRef Definition => this.IsDefinition ? this : this.Builder.ToRef();

    public IDeclarationBuilder Builder { get; }

    public GenericContext GenericContext { get; } // Gives the type arguments for the builder.

    public override ICompilationBoundRefImpl WithGenericContext( GenericContext genericContext )
        => genericContext.IsEmptyOrIdentity ? this : new BuilderRef<T>( this.Builder, genericContext, this.CompilationContext );

    public override IRefCollectionStrategy CollectionStrategy => BuilderRefCollectionStrategy.Instance;

    public override string Name
        => this.Builder switch
        {
            INamedDeclaration named => named.Name,
            _ => throw new NotSupportedException( $"Declarations of kind {this.Builder.DeclarationKind} have no name." )
        };

    public override SerializableDeclarationId ToSerializableId() => this.Builder.ToSerializableId();

    public override SerializableDeclarationId ToSerializableId( CompilationContext compilationContext ) => this.Builder.ToSerializableId();

    protected override ISymbol GetSymbolIgnoringRefKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
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

    private GenericContext SelectGenericContext( IGenericContext? genericContext )
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
        bool throwIfMissing,
        IGenericContext? genericContext )
        => ConvertDeclarationOrThrow(
            compilation.Factory.GetDeclaration( this.Builder, this.SelectGenericContext( genericContext ), typeof(T) ),
            compilation );

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
        => this switch
        {
            IRefImpl<TOut> desired => desired,
            IRef<IField> when this.Builder is PromotedField && typeof(TOut) == typeof(IProperty) =>
                (IRefImpl<TOut>) (object) new BuilderRef<IProperty>( this.Builder, this.GenericContext, this.CompilationContext ),
            IRef<IProperty> when this.Builder is PromotedField && typeof(TOut) == typeof(IField) =>
                (IRefImpl<TOut>) (object) new BuilderRef<IField>( this.Builder, this.GenericContext, this.CompilationContext ),
            _ => throw new InvalidCastException( $"Cannot convert the IRef<{typeof(T).Name}> to IRef<{typeof(TOut).Name}>) for '{this}'." )
        };
}