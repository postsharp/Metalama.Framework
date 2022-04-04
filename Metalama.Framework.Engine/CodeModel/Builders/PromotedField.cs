// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PromotedField : PropertyBuilder, IReplaceMember
    {
        private readonly IField _field;

        public MemberRef<IMemberOrNamedType> ReplacedMember => this._field.ToMemberRef<IMemberOrNamedType>();

        public PromotedField( Advice advice, IField field ) : base(
            advice,
            field.DeclaringType,
            field.Name,
            true,
            field.Writeability == Writeability.All,
            true,
            false )
        {
            this._field = field;
            this.Type = field.Type;
            this.Accessibility = this._field.Accessibility;
            this.IsStatic = this._field.IsStatic;

            // TODO: Attributes etc.
        }

        public override InsertPosition InsertPosition => this._field.ToInsertPosition();

        public override SyntaxTree TargetSyntaxTree
            => this._field switch
            {
                ISyntaxTreeTransformation introduction => introduction.TargetSyntaxTree,
                BuiltDeclaration builtDeclaration => ((ISyntaxTreeTransformation) builtDeclaration.Builder).TargetSyntaxTree,
                Declaration codeDeclaration => codeDeclaration.GetSymbol().AssertNotNull().GetPrimarySyntaxReference().AssertNotNull().SyntaxTree,
                _ => throw new AssertionFailedException(),
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
    }
}