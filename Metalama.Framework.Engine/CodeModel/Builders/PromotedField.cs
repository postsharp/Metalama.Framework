// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PromotedField : PropertyBuilder, IReplaceMemberTransformation
    {
        private readonly IFieldImpl _field;

        public MemberRef<IMember> ReplacedMember => this._field.ToMemberRef<IMember>();

        public override Writeability Writeability => this._field.Writeability;

        public PromotedField( IServiceProvider serviceProvider, Advice advice, IField field, IObjectReader tags ) : base(
            advice,
            field.DeclaringType,
            field.Name,
            true,
            true,
            true,
            false,
            true,
            true,
            tags )
        {
            this._field = (IFieldImpl) field;
            this.Type = field.Type;
            this.Accessibility = this._field.Accessibility;
            this.IsStatic = this._field.IsStatic;

            this.GetMethod.AssertNotNull().Accessibility = this._field.Accessibility;
            this.SetMethod.AssertNotNull().Accessibility = this._field.Accessibility;

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

        public override SyntaxTree? PrimarySyntaxTree => this._field.PrimarySyntaxTree;

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