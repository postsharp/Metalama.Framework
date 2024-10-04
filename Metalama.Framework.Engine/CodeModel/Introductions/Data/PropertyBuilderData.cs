// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class PropertyBuilderData : PropertyOrIndexerBuilderData
{
    public ImmutableArray<IAttributeData> FieldAttributes { get; }

    public IExpression? InitializerExpression { get; }

    public TemplateMember<IProperty>? InitializerTemplate { get; }

    public bool IsAutoPropertyOrField { get; }

    public IObjectReader InitializerTags { get; }

    public IRef<IProperty>? OverriddenProperty { get; }
    
    public IReadOnlyList<IRef<IProperty>> ExplicitInterfaceImplementations { get; }

    public PropertyBuilderData( PropertyBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this.FieldAttributes = builder.FieldAttributes.ToImmutableArray();
        this.InitializerExpression = builder.InitializerExpression;
        this.IsAutoPropertyOrField = builder.IsAutoPropertyOrField;
        this.OverriddenProperty = builder.OverriddenProperty?.ToRef();
        this.InitializerTemplate = builder.InitializerTemplate;
        this.ExplicitInterfaceImplementations = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );

        // TODO: Potential CompilationModel leak
        this.InitializerTags = builder.InitializerTags;
    }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Property;

    public override IRef<IMember>? OverriddenMember => this.OverriddenProperty;
    
    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementations;
    
}