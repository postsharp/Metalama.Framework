// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Built;
using Metalama.Framework.Engine.CodeModel.Introductions.Helpers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

/// <summary>
/// Represents a property that has been created from a field. It implements both the <see cref="IField"/> and <see cref="IProperty"/>
/// interfaces.
/// </summary>
internal sealed class PromotedFieldBuilder : PropertyBuilder
{
    /// <summary>
    /// Gets the original <see cref="Field"/> or <see cref="FieldBuilder"/>.
    /// </summary>
    public IFieldImpl OriginalSourceFieldOrBuiltField { get; }

    public static PromotedFieldBuilder Create(
        in ProjectServiceProvider serviceProvider,
        IField field,
        IObjectReader initializerTags,
        AspectLayerInstance aspectLayerInstance )
        => new(
            serviceProvider,
            field,
            initializerTags,
            aspectLayerInstance );

    private PromotedFieldBuilder(
        in ProjectServiceProvider serviceProvider,
        IField field,
        IObjectReader initializerTags,
        AspectLayerInstance aspectLayerInstance ) : base(
        aspectLayerInstance,
        field.DeclaringType,
        field.Name,
        true,
        true,
        true,
        field is { IsStatic: false, Writeability: Writeability.ConstructorOnly },
        true,
        true,
        initializerTags )
    {
        Invariant.Assert( field is (Field or BuiltField) );

        this.OriginalSourceFieldOrBuiltField = (IFieldImpl) field;
        this.Type = field.Type;
        this.Accessibility = this.OriginalSourceFieldOrBuiltField.Accessibility;
        this.IsStatic = this.OriginalSourceFieldOrBuiltField.IsStatic;
        this.IsRequired = this.OriginalSourceFieldOrBuiltField.IsRequired;
        this.IsNew = this.OriginalSourceFieldOrBuiltField.IsNew;
        this.HasNewKeyword = this.OriginalSourceFieldOrBuiltField.HasNewKeyword.AssertNotNull();

        this.GetMethod.AssertNotNull().Accessibility = this.OriginalSourceFieldOrBuiltField.Accessibility;

        this.SetMethod.AssertNotNull().Accessibility =
            this.OriginalSourceFieldOrBuiltField switch
            {
                { Writeability: Writeability.ConstructorOnly } => Accessibility.Private,
                _ => this.OriginalSourceFieldOrBuiltField.Accessibility
            };

        if ( field.Attributes.Count > 0 )
        {
            var classificationService = serviceProvider.Global.GetRequiredService<AttributeClassificationService>();

            foreach ( var attribute in field.Attributes )
            {
                if ( classificationService.MustMoveFromFieldToProperty(
                        attribute.Type.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.IntroducedAttributeTypes ) ) )
                {
                    this.AddAttribute( attribute.ToAttributeConstruction() );
                }
                else
                {
                    this.AddFieldAttribute( attribute.ToAttributeConstruction() );
                }
            }
        }

        this.Freeze();
    }

    public override Writeability Writeability
        => this.OriginalSourceFieldOrBuiltField.Writeability switch
        {
            Writeability.None => Writeability.None,
            Writeability.ConstructorOnly => Writeability.InitOnly, // Read-only fields are promoted to init-only properties.
            Writeability.All => Writeability.All,
            _ => throw new AssertionFailedException( $"Unexpected Writeability: {this.OriginalSourceFieldOrBuiltField.Writeability}." )
        };

    public override SyntaxTree? PrimarySyntaxTree => this.OriginalSourceFieldOrBuiltField.PrimarySyntaxTree;

    public override IInjectMemberTransformation ToTransformation()
        => new PromoteFieldTransformation( this.AspectLayerInstance, this.OriginalSourceFieldOrBuiltField, this.Immutable );

    public override bool Equals( IDeclaration? other )
        => ReferenceEquals( this, other ) || (other is PromotedFieldBuilder otherPromotedField
                                              && otherPromotedField.OriginalSourceFieldOrBuiltField.Equals( this.OriginalSourceFieldOrBuiltField ));

    public override bool IsDesignTimeObservable => false;

    public FieldInfo ToFieldInfo() => throw new NotImplementedException();

    public TypedConstant? ConstantValue => this.OriginalSourceFieldOrBuiltField.ConstantValue;

}