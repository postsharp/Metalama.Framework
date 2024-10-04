// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class NamedTypeBuilderData : MemberOrNamedTypeBuilderData
{
    public IRef<INamedType>? BaseType { get; }

    public ImmutableArray<TypeParameterBuilderData> TypeParameters { get; }


    public NamedTypeBuilderData( NamedTypeBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        var me = this.ToDeclarationRef();
        this.BaseType = builder.BaseType?.ToRef();
        this.TypeParameters = builder.TypeParameters.ToImmutable(me);
    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new System.NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;
}

internal class NamespaceBuilderData : NamedDeclarationBuilderData
{
    public NamespaceBuilderData( NamespaceBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration ) { }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new System.NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;
}