// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;

internal class NamespaceBuilderData : NamedDeclarationBuilderData
{
    private readonly IntroducedRef<INamespace> _ref;

    public NamespaceBuilderData( NamespaceBuilder builder, IFullRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new IntroducedRef<INamespace>( this, containingDeclaration.RefFactory );
        this.Attributes = ImmutableArray<AttributeBuilderData>.Empty;
    }

    protected override IFullRef<IDeclaration> ToDeclarationRef() => this._ref;

    public override IFullRef<INamedType>? DeclaringType => null;

    public new IFullRef<INamespace> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Namespace;
}