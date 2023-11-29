// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Accessibility = Metalama.Framework.Code.Accessibility;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class PromotedField : PropertyBuilder
    {
        internal IFieldImpl Field { get; }

        public override Ref<IDeclaration> ToRef() => this.Field.ToRef();

        public override Writeability Writeability
            => this.Field.Writeability switch
            {
                Writeability.None => Writeability.None,
                Writeability.ConstructorOnly => Writeability.InitOnly, // Read-only fields are promoted to init-only properties.
                Writeability.All => Writeability.All,
                _ => throw new AssertionFailedException( $"Unexpected Writeability: {this.Field.Writeability}." )
            };

        public PromotedField( ProjectServiceProvider serviceProvider, IField field, IObjectReader initializerTags, Advice advice ) : base(
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
            this.Field = (IFieldImpl) field;
            this.Type = field.Type;
            this.Accessibility = this.Field.Accessibility;
            this.IsStatic = this.Field.IsStatic;
            this.IsRequired = this.Field.IsRequired;
            this.IsNew = this.Field.IsNew;
            this.HasNewKeyword = this.Field.HasNewKeyword.AssertNotNull();

            this.GetMethod.AssertNotNull().Accessibility = this.Field.Accessibility;

            this.SetMethod.AssertNotNull().Accessibility =
                this.Field switch
                {
                    { Writeability: Writeability.ConstructorOnly } => Accessibility.Private,
                    _ => this.Field.Accessibility
                };

            if ( field.Attributes.Count > 0 )
            {
                var classificationService = serviceProvider.Global.GetRequiredService<AttributeClassificationService>();

                foreach ( var attribute in field.Attributes )
                {
                    if ( classificationService.MustMoveFromFieldToProperty( attribute.Type.GetSymbol() ) )
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

        public override SyntaxTree? PrimarySyntaxTree => this.Field.PrimarySyntaxTree;

        protected internal override bool GetPropertyInitializerExpressionOrMethod(
            Advice advice,
            MemberInjectionContext context,
            out ExpressionSyntax? initializerExpression,
            out MethodDeclarationSyntax? initializerMethod )
        {
            if ( this.Field is BuiltField builtField )
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
                var fieldDeclaration = (VariableDeclaratorSyntax) this.Field.GetPrimaryDeclarationSyntax().AssertNotNull();

                if ( fieldDeclaration.Initializer != null )
                {
                    initializerExpression = fieldDeclaration.Initializer.Value;
                }
                else if ( this.DeclaringType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct )
                {
                    // In structs, we have to initialize all introduced fields.
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

        public override IInjectMemberTransformation ToTransformation() => new ReplaceWithPropertyTransformation( this.ParentAdvice, this.Field, this );

        public override bool Equals( IDeclaration? other )
            => ReferenceEquals( this, other ) || (other is PromotedField otherPromotedField && otherPromotedField.Field.Equals( this.Field ));
    }
}