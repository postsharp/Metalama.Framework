// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class AttributeBuilderData : DeclarationBuilderData
{
    private readonly DeclarationBuilderDataRef<IAttribute> _ref;

    public IRef<INamedType> Type { get; }

    public IRef<IConstructor> Constructor { get; }

    public ImmutableArray<TypedConstant> ConstructorArguments { get; }

    public INamedArgumentList NamedArguments { get; }

    public AttributeBuilderData( AttributeBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new DeclarationBuilderDataRef<IAttribute>( this );
        this.Constructor = builder.AttributeConstruction.Constructor.ToRef();
        this.Type = builder.AttributeConstruction.Type.ToRef();
        this.ConstructorArguments = builder.AttributeConstruction.ConstructorArguments.ToImmutableArray();
        this.NamedArguments = builder.AttributeConstruction.NamedArguments;

        // TODO: TypedConstant can still leak a CompilationModel through its typeof(.) value.
    }

    protected override IRef<IDeclaration> ToDeclarationRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Attribute;

    public new IRef<IAttribute> ToRef() => this._ref;
}