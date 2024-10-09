// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Built;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class PropertyBuilderData : PropertyOrIndexerBuilderData
{
    private readonly BuiltDeclarationRef<IProperty> _ref;

    public ImmutableArray<IAttributeData> FieldAttributes { get; }

    public IExpression? InitializerExpression { get; }

    public TemplateMember<IProperty>? InitializerTemplate { get; }

    public bool IsAutoPropertyOrField { get; }

    public IObjectReader InitializerTags { get; }

    public IRef<IProperty>? OverriddenProperty { get; }

    public IReadOnlyList<IRef<IProperty>> ExplicitInterfaceImplementations { get; }

    public override MethodBuilderData? GetMethod { get; }

    public override MethodBuilderData? SetMethod { get; }

    public bool IsRequired { get; }

    private readonly object? _originalField; // Can be an IFieldSymbol or a FieldBuilderData.

    public PropertyBuilderData( PropertyBuilder builder, IFullRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new BuiltDeclarationRef<IProperty>( this, containingDeclaration.CompilationContext );
        this.FieldAttributes = builder.FieldAttributes.ToImmutableArray();
        this.InitializerExpression = builder.InitializerExpression;
        this.IsAutoPropertyOrField = builder.IsAutoPropertyOrField;
        this.OverriddenProperty = builder.OverriddenProperty?.ToRef();
        this.InitializerTemplate = builder.InitializerTemplate;
        this.ExplicitInterfaceImplementations = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );
        this.IsRequired = builder.IsRequired;

        if ( builder.OriginalField != null )
        {
            Invariant.Assert( builder.OriginalField.GenericContext.IsEmptyOrIdentity );

            this._originalField = builder.OriginalField switch
            {
                Field sourceField => sourceField.Symbol,
                BuiltField builtField => builtField.FieldBuilderData,
                _ => throw new AssertionFailedException()
            };
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
    }

    protected override IFullRef<IDeclaration> ToDeclarationRef() => this._ref;

    public new BuiltDeclarationRef<IProperty> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Property;

    public override IRef<IMember>? OverriddenMember => this.OverriddenProperty;

    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementations;

    public IField GetOriginalField( CompilationModel compilation, GenericContext genericContext )
        => this._originalField switch
        {
            IFieldSymbol fieldSymbol => compilation.Factory.GetField( fieldSymbol ),
            FieldBuilderData fieldBuilderData => compilation.Factory.GetField( fieldBuilderData, genericContext ),
            _ => throw new AssertionFailedException()
        };

    public bool GetPropertyInitializerExpressionOrMethod(
        IProperty property,
        PropertyBuilderData builderData,
        AspectLayerInstance aspectLayerInstance,
        MemberInjectionContext context,
        out ExpressionSyntax? initializerExpression,
        out MethodDeclarationSyntax? initializerMethod )
    {
        switch ( this._originalField )
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

            case FieldBuilderData fieldBuilderData:
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

            default:
                throw new AssertionFailedException();
        }
    }

    protected override InsertPosition GetInsertPosition()
    {
        switch ( this._originalField )
        {
            case null:
                return base.GetInsertPosition();

            case ISymbol symbol:
                return symbol.ToInsertPosition();

            case DeclarationBuilderData builderData:
                return builderData.InsertPosition;

            default:
                throw new AssertionFailedException();
        }
    }
}