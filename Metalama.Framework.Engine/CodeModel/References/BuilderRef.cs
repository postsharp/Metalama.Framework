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
    public BuilderRef( IDeclarationBuilder builder )
    {
        this.Builder = builder;
    }

    private protected override CompilationContext? CompilationContext => this.Builder.GetCompilationContext();

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

    protected override ISymbol GetSymbolIgnoringKind( CompilationContext compilationContext, bool ignoreAssemblyKey = false )
    {
        throw new NotSupportedException();
    }

    public override ISymbol GetClosestSymbol( CompilationContext compilationContext )
    {
        var containingDeclaration = this.Builder.ContainingDeclaration;

        // This can happen for accessor method of a builder member.
        if ( containingDeclaration is IDeclarationBuilder containingBuilder )
        {
            containingDeclaration = containingBuilder.ContainingDeclaration;
        }

        return containingDeclaration.AssertNotNull().GetSymbol( compilationContext ).AssertSymbolNotNull();
    }

    protected override T? Resolve( CompilationModel compilation, ReferenceResolutionOptions options, bool throwIfMissing, IGenericContext? genericContext )
    {
        return this.ConvertOrThrow( compilation.Factory.GetDeclaration( this.Builder, options, genericContext, throwIfMissing ), compilation );
    }

    public override bool Equals( IRef? other ) => other.Unwrap() is IBuilderRef builderRef && this.Builder == builderRef.Builder;

    protected override int GetHashCodeCore() => this.Builder.GetHashCode();

    public override string ToString() => this.Builder.ToDisplayString();
}