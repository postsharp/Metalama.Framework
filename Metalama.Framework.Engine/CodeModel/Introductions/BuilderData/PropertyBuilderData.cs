// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;

internal class PropertyBuilderData : PropertyOrIndexerBuilderData
{
    private readonly IntroducedRef<IProperty> _ref;

    public ImmutableArray<IAttributeData> FieldAttributes { get; }

    public IExpression? InitializerExpression { get; }

    public TemplateMember<IProperty>? InitializerTemplate { get; }

    public bool IsAutoPropertyOrField { get; }

    public IObjectReader InitializerTags { get; }

    public IRef<IProperty>? OverriddenProperty { get; }

    public IReadOnlyList<IRef<IProperty>> ExplicitInterfaceImplementations { get; }

    public override MethodBuilderData? GetMethod { get; }

    public override MethodBuilderData? SetMethod { get; }

    public IFullRef<IField>? OriginalField { get; }

    public bool IsRequired { get; }

    public PropertyBuilderData( PropertyBuilder builder, IFullRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new IntroducedRef<IProperty>( this, containingDeclaration.RefFactory );
        this.FieldAttributes = builder.FieldAttributes.ToImmutableArray();
        this.InitializerExpression = builder.InitializerExpression;
        this.IsAutoPropertyOrField = builder.IsAutoPropertyOrField;
        this.OverriddenProperty = builder.OverriddenProperty?.ToRef();
        this.InitializerTemplate = builder.InitializerTemplate;
        this.ExplicitInterfaceImplementations = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );
        this.IsRequired = builder.IsRequired;
        this.OriginalField = builder.OriginalField?.ToFullRef();

        if ( builder.OriginalField != null )
        {
            Invariant.Assert( builder.OriginalField.GenericContext.IsEmptyOrIdentity );
        }

        // TODO: Potential CompilationModel leak
        this.InitializerTags = builder.InitializerTags;

        if ( builder.GetMethod != null )
        {
            this.GetMethod = new MethodBuilderData( builder.GetMethod, this._ref );
        }

        if ( builder.SetMethod != null )
        {
            this.SetMethod = new MethodBuilderData( builder.SetMethod, this._ref );
        }

        this.Attributes = builder.Attributes.ToImmutable( this._ref );
    }

    protected override IFullRef<IDeclaration> ToDeclarationFullRef() => this._ref;

    public new IntroducedRef<IProperty> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Property;

    public override IRef<IMember>? OverriddenMember => this.OverriddenProperty;

    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementations;

    // TODO: It probably does not belong here.
    public bool GetPropertyInitializerExpressionOrMethod(
        IProperty property,
        PropertyBuilderData builderData,
        AspectLayerInstance aspectLayerInstance,
        MemberInjectionContext context,
        out ExpressionSyntax? initializerExpression,
        out MethodDeclarationSyntax? initializerMethod )
    {
        switch ( this.OriginalField )
        {
            case null:
                return AdviceSyntaxGenerator.GetInitializerExpressionOrMethod(
                    property,
                    aspectLayerInstance,
                    context,
                    property.Type,
                    property.InitializerExpression,
                    builderData.InitializerTemplate,
                    builderData.InitializerTags,
                    out initializerExpression,
                    out initializerMethod );

            case IIntroducedRef { BuilderData: FieldBuilderData fieldBuilderData }:
                return AdviceSyntaxGenerator.GetInitializerExpressionOrMethod(
                    property.OriginalField.AssertNotNull(),
                    aspectLayerInstance,
                    context,
                    property.Type,
                    fieldBuilderData.InitializerExpression,
                    fieldBuilderData.InitializerTemplate,
                    builderData.InitializerTags,
                    out initializerExpression,
                    out initializerMethod );

            case ISymbolRef:
                // TODO: Not sure what we should do here.
                initializerExpression = null;
                initializerMethod = null;

                return false;

            default:
                throw new AssertionFailedException();
        }
    }

    protected override InsertPosition GetInsertPosition()
    {
        switch ( this.OriginalField )
        {
            case null:
                return base.GetInsertPosition();

            case ISymbolRef symbolRef:
                return symbolRef.Symbol.ToInsertPosition();

            case IIntroducedRef builtDeclarationRef:
                return builtDeclarationRef.BuilderData.InsertPosition;

            default:
                throw new AssertionFailedException();
        }
    }
}