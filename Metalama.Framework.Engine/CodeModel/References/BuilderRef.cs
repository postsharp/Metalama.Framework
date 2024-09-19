// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class BuilderRef<T> : BaseRef<T>, IBuilderRef
    where T : class, ICompilationElement
{
    public BuilderRef( IDeclarationBuilder builder, CompilationContext compilationContext )
    {
        Invariant.Assert( typeof(T) == builder.DeclarationKind.GetRefInterfaceType() );

        this.Builder = builder;
        this.CompilationContext = compilationContext;
    }

    public override CompilationContext CompilationContext { get; }

    public IDeclarationBuilder Builder { get; }

    IDeclaration IDeclarationRef.Declaration => this.Builder;

    public override IRefStrategy Strategy => DeclarationRefStrategy.Instance;

    public override string Name
        => this.Builder switch
        {
            INamedDeclaration named => named.Name,
            _ => throw new NotSupportedException( $"Declarations of kind {this.Builder.DeclarationKind} have no name." )
        };

    public override SerializableDeclarationId ToSerializableId() => this.Builder.ToSerializableId();

    protected override ISymbol GetSymbolIgnoringKind( bool ignoreAssemblyKey = false )
    {
        throw new NotSupportedException();
    }

    public override ISymbol GetClosestSymbol()
    {
        var containingDeclaration = this.Builder.ContainingDeclaration;

        // This can happen for accessor method of a builder member.
        if ( containingDeclaration is IDeclarationBuilder containingBuilder )
        {
            containingDeclaration = containingBuilder.ContainingDeclaration;
        }

        return containingDeclaration.AssertNotNull().GetSymbol( this.CompilationContext ).AssertSymbolNotNull();
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        return ConvertOrThrow( compilation.Factory.GetDeclaration( this.Builder, options, genericContext, throwIfMissing ), compilation );
    }

    public override bool Equals( IRef? other ) => other is IBuilderRef builderRef && this.Builder == builderRef.Builder;

    protected override int GetHashCodeCore() => this.Builder.GetHashCode();

    public override string ToString() => this.Builder.ToString();

    public override IRefImpl<TOut> As<TOut>()
        => (IRefImpl<TOut>) this; // There should be no reason to upcast since we always create instances of the right type.
}