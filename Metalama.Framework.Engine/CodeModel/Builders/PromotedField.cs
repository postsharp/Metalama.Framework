// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Accessibility = Metalama.Framework.Code.Accessibility;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PromotedField : PropertyBuilder, IReplaceMemberTransformation
    {
        private readonly IFieldImpl _field;

        public MemberRef<IMember> ReplacedMember => this._field.ToMemberRef<IMember>();

        public override Writeability Writeability
            => this._field.Writeability switch
            {
                Writeability.None => Writeability.None,
                Writeability.ConstructorOnly => Writeability.InitOnly, // Read-only fields are promoted to init-only properties.
                Writeability.All => Writeability.All,
                _ => throw new AssertionFailedException( $"Unexpected Writeability: {this._field.Writeability}." )
            };

        public PromotedField( IServiceProvider serviceProvider, Advice advice, IField field, IObjectReader initializerTags ) : base(
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
            this._field = (IFieldImpl) field;
            this.Type = field.Type;
            this.Accessibility = this._field.Accessibility;
            this.IsStatic = this._field.IsStatic;

            this.GetMethod.AssertNotNull().Accessibility = this._field.Accessibility;

            this.SetMethod.AssertNotNull().Accessibility =
                this._field switch
                {
                    { Writeability: Writeability.ConstructorOnly } => Accessibility.Private,
                    _ => this._field.Accessibility
                };

            if ( field.Attributes.Count > 0 )
            {
                var classificationService = serviceProvider.GetRequiredService<AttributeClassificationService>();

                foreach ( var attribute in field.Attributes )
                {
                    if ( classificationService.MustMoveFromFieldToProperty( attribute.Type.GetSymbol() ) )
                    {
                        this.AddAttribute( attribute.ToAttributeConstruction() );
                    }
                }
            }
        }

        public override IDeclaration TargetDeclaration => this._field;

        public override bool IsDesignTime => false;

        protected override bool HasBaseInvoker => true;

        protected override bool GetPropertyInitializerExpressionOrMethod(
            in MemberIntroductionContext context,
            out ExpressionSyntax? initializerExpression,
            out MethodDeclarationSyntax? initializerMethod )
        {
            if ( this._field is BuiltField builtField )
            {
                var fieldBuilder = builtField.FieldBuilder;

                return fieldBuilder.GetInitializerExpressionOrMethod(
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
                var fieldDeclaration = (VariableDeclaratorSyntax) this._field.GetPrimaryDeclarationSyntax().AssertNotNull();

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
    }
}