// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using Accessibility = Metalama.Framework.Code.Accessibility;
using TypedConstant = Metalama.Framework.Code.TypedConstant;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

/// <summary>
/// Represents a property that has been created from a field. It implements both the <see cref="IField"/> and <see cref="IProperty"/>
/// interfaces.
/// </summary>
internal sealed class PromotedField : PropertyBuilder, IFieldImpl, IFieldBuilder
{
    /// <summary>
    /// Gets the original <see cref="Field"/> or <see cref="FieldBuilder"/>.
    /// </summary>
    public IFieldImpl OriginalSourceFieldOrFieldBuilder { get; }

    public static PromotedField Create( in ProjectServiceProvider serviceProvider, IField field, IObjectReader initializerTags, Advice advice )
        => new(
            serviceProvider,
            field switch
            {
                BuiltField builtField => builtField.FieldBuilder,
                _ => field
            },
            initializerTags,
            advice );

    private PromotedField( in ProjectServiceProvider serviceProvider, IField field, IObjectReader initializerTags, Advice advice ) : base(
        advice,
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
        Invariant.Assert( field is Field or FieldBuilder );

        this.OriginalSourceFieldOrFieldBuilder = (IFieldImpl) field;
        this.Type = field.Type;
        this.Accessibility = this.OriginalSourceFieldOrFieldBuilder.Accessibility;
        this.IsStatic = this.OriginalSourceFieldOrFieldBuilder.IsStatic;
        this.IsRequired = this.OriginalSourceFieldOrFieldBuilder.IsRequired;
        this.IsNew = this.OriginalSourceFieldOrFieldBuilder.IsNew;
        this.HasNewKeyword = this.OriginalSourceFieldOrFieldBuilder.HasNewKeyword.AssertNotNull();

        this.GetMethod.AssertNotNull().Accessibility = this.OriginalSourceFieldOrFieldBuilder.Accessibility;

        this.SetMethod.AssertNotNull().Accessibility =
            this.OriginalSourceFieldOrFieldBuilder switch
            {
                { Writeability: Writeability.ConstructorOnly } => Accessibility.Private,
                _ => this.OriginalSourceFieldOrFieldBuilder.Accessibility
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
    }

    public override Writeability Writeability
        => this.OriginalSourceFieldOrFieldBuilder.Writeability switch
        {
            Writeability.None => Writeability.None,
            Writeability.ConstructorOnly => Writeability.InitOnly, // Read-only fields are promoted to init-only properties.
            Writeability.All => Writeability.All,
            _ => throw new AssertionFailedException( $"Unexpected Writeability: {this.OriginalSourceFieldOrFieldBuilder.Writeability}." )
        };

    public override SyntaxTree? PrimarySyntaxTree => this.OriginalSourceFieldOrFieldBuilder.PrimarySyntaxTree;

    protected internal override bool GetPropertyInitializerExpressionOrMethod(
        Advice advice,
        MemberInjectionContext context,
        out ExpressionSyntax? initializerExpression,
        out MethodDeclarationSyntax? initializerMethod )
    {
        if ( this.OriginalSourceFieldOrFieldBuilder is FieldBuilder fieldBuilder )
        {
            return fieldBuilder.GetInitializerExpressionOrMethod(
                fieldBuilder.ParentAdvice,
                context,
                this.Type,
                fieldBuilder.InitializerExpression,
                fieldBuilder.InitializerTemplate,
                this.InitializerTags,
                out initializerExpression,
                out initializerMethod );
        }
        else
        {
            // For original code fields, copy the initializer syntax.
            var fieldDeclaration = (VariableDeclaratorSyntax) this.OriginalSourceFieldOrFieldBuilder.GetPrimaryDeclarationSyntax().AssertNotNull();

            if ( fieldDeclaration.Initializer != null )
            {
                initializerExpression = fieldDeclaration.Initializer.Value;
            }
            else if ( this.DeclaringType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct
                      && context.SyntaxGenerationContext.RequiresStructFieldInitialization )
            {
                // In structs in C# 10, we have to initialize all introduced fields.
                initializerExpression = SyntaxFactoryEx.Default;
            }
            else
            {
                initializerExpression = null;
            }

            initializerMethod = null;

            return true;
        }
    }

    public override IInjectMemberTransformation ToTransformation()
        => new PromoteFieldTransformation( this.ParentAdvice, this.OriginalSourceFieldOrFieldBuilder, this );

    public override bool Equals( IDeclaration? other )
        => ReferenceEquals( this, other ) || (other is PromotedField otherPromotedField
                                              && otherPromotedField.OriginalSourceFieldOrFieldBuilder.Equals( this.OriginalSourceFieldOrFieldBuilder ));

    public override bool IsDesignTimeObservable => false;

    public FieldInfo ToFieldInfo() => throw new NotImplementedException();

    public TypedConstant? ConstantValue => this.OriginalSourceFieldOrFieldBuilder.ConstantValue;

    public IField Definition => this;

    [Memo]
    public CompilationBoundRef<IField> FieldRef => (CompilationBoundRef<IField>) this.OriginalSourceFieldOrFieldBuilder.ToRef();

    IRef<IField> IField.ToRef() => this.FieldRef;

    public IProperty? OverridingProperty => this;

    public override IField OriginalField => this;
}