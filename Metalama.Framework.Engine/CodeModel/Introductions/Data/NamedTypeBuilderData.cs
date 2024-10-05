// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class NamedTypeBuilderData : MemberOrNamedTypeBuilderData
{
    private readonly IRef<INamedType> _ref;
    
    public IRef<INamedType>? BaseType { get; }

    public ImmutableArray<TypeParameterBuilderData> TypeParameters { get; }
    
    // Only classes are supported at the moment, so the following members can return a constant value.

    public TypeKind TypeKind => TypeKind.Class;
    
    public bool IsReadOnly => false;

    public bool IsRef => false;


    public NamedTypeBuilderData( NamedTypeBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new DeclarationBuilderDataRef<INamedType>( this);
        this.BaseType = builder.BaseType?.ToRef();
        this.TypeParameters = builder.TypeParameters.ToImmutable(this._ref);
    }

    protected override IRef<IDeclaration> ToDeclarationRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;
}