// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = Metalama.Framework.Code.Accessibility;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class PromotedField : PropertyBuilder
{
    public override IField OriginalField { get; }

    public override Writeability Writeability
        => this.OriginalField.Writeability switch
        {
            Writeability.None => Writeability.None,
            Writeability.ConstructorOnly => Writeability.InitOnly, // Read-only fields are promoted to init-only properties.
            Writeability.All => Writeability.All,
            _ => throw new AssertionFailedException( $"Unexpected Writeability: {this.OriginalField.Writeability}." )
        };

    public PromotedField( in ProjectServiceProvider serviceProvider, IField field, IObjectReader initializerTags, Advice advice ) : base(
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
        this.OriginalField = (IFieldImpl) field;
        this.Type = field.Type;
        this.Accessibility = this.OriginalField.Accessibility;
        this.IsStatic = this.OriginalField.IsStatic;
        this.IsRequired = this.OriginalField.IsRequired;
        this.IsNew = this.OriginalField.IsNew;
        this.HasNewKeyword = ((IFieldImpl) this.OriginalField).HasNewKeyword.AssertNotNull();

        this.GetMethod.AssertNotNull().Accessibility = this.OriginalField.Accessibility;

        this.SetMethod.AssertNotNull().Accessibility =
            this.OriginalField switch
            {
                { Writeability: Writeability.ConstructorOnly } => Accessibility.Private,
                _ => this.OriginalField.Accessibility
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

    public override SyntaxTree? PrimarySyntaxTree => ((IFieldImpl) this.OriginalField).PrimarySyntaxTree;

    protected internal override bool GetPropertyInitializerExpressionOrMethod(
        Advice advice,
        MemberInjectionContext context,
        out ExpressionSyntax? initializerExpression,
        out MethodDeclarationSyntax? initializerMethod )
    {
        if ( this.OriginalField is BuiltField builtField )
        {
            var fieldBuilder = builtField.FieldBuilder;

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
            var fieldDeclaration = (VariableDeclaratorSyntax) this.OriginalField.GetPrimaryDeclarationSyntax().AssertNotNull();

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

    public override IInjectMemberTransformation ToTransformation() => new PromoteFieldTransformation( this.ParentAdvice, this.OriginalField, this );

    public override bool Equals( IDeclaration? other )
        => ReferenceEquals( this, other ) || (other is PromotedField otherPromotedField && otherPromotedField.OriginalField.Equals( this.OriginalField ));

    public override bool IsDesignTimeObservable => false;
}