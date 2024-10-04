// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class PropertyBuilderData : PropertyOrIndexerBuilderData
{
    public ImmutableArray<IAttributeData> FieldAttributes { get; }

    public IExpression? InitializerExpression { get; }

    public TemplateMember<IProperty>? InitializerTemplate { get; }

    public bool IsAutoPropertyOrField { get; }

    public IObjectReader InitializerTags { get; }

    public IRef<IProperty>? OverriddenProperty { get; }

    public PropertyBuilderData( PropertyBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this.FieldAttributes = builder.FieldAttributes.ToImmutableArray();
        this.InitializerExpression = builder.InitializerExpression;
        this.IsAutoPropertyOrField = builder.IsAutoPropertyOrField;
        this.OverriddenProperty = builder.OverriddenProperty?.ToRef();
        this.InitializerTemplate = builder.InitializerTemplate;

        // TODO: Potential CompilationModel leak
        this.InitializerTags = builder.InitializerTags;
    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Property;
}