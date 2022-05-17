// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PromotedField : PropertyBuilder, IReplaceMemberTransformation
    {
        private readonly IField _field;

        public MemberRef<IMember>? ReplacedMember => this._field.ToMemberRef<IMember>();

        public PromotedField( Advice advice, IField field, IObjectReader tags ) : base(
            advice,
            field.DeclaringType,
            field.Name,
            true,
            field.Writeability == Writeability.All,
            true,
            false,
            tags )
        {
            this._field = field;
            this.Type = field.Type;
            this.Accessibility = this._field.Accessibility;
            this.IsStatic = this._field.IsStatic;

            foreach ( var attribute in field.Attributes )
            {
                this.AddAttribute( attribute.ToAttributeConstruction() );
            }
        }

        public override InsertPosition InsertPosition => this._field.ToInsertPosition();

        public override SyntaxTree TargetSyntaxTree
            => this._field switch
            {
                IDeclarationImpl declaration => declaration.PrimarySyntaxTree.AssertNotNull(),
                _ => throw new AssertionFailedException()
            };

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
                var fieldDeclaration = (VariableDeclaratorSyntax) this._field.GetPrimaryDeclaration().AssertNotNull();

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

        protected override SyntaxList<AttributeListSyntax> GetAttributeLists( in SyntaxGenerationContext syntaxGenerationContext )
        {
            // TODO: 
            return List<AttributeListSyntax>();
        }
    }
}