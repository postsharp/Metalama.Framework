// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class AttributeBuilderData : DeclarationBuilderData
{
    private readonly AttributeRef _ref;

    public IFullRef<INamedType> Type { get; }

    public IFullRef<IConstructor> Constructor { get; }

    public ImmutableArray<TypedConstant> ConstructorArguments { get; }

    public INamedArgumentList NamedArguments { get; }

    public AttributeBuilderData( AttributeBuilder builder, IFullRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        // Type must be set before the Ref is created.
        this.Type = builder.AttributeConstruction.Type.ToFullRef();

        this._ref = new BuilderAttributeRef( this );

        this.Constructor = builder.AttributeConstruction.Constructor.ToFullRef();
        this.ConstructorArguments = builder.AttributeConstruction.ConstructorArguments.ToImmutableArray();
        this.NamedArguments = builder.AttributeConstruction.NamedArguments;
        this.Attributes = ImmutableArray<AttributeBuilderData>.Empty;

        // TODO: TypedConstant can still leak a CompilationModel through its typeof(.) value.
    }

    protected override IFullRef<IDeclaration> ToDeclarationRef() => throw new NotSupportedException();

    public override IFullRef<INamedType>? DeclaringType => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Attribute;

    public new AttributeRef ToRef() => this._ref;
}