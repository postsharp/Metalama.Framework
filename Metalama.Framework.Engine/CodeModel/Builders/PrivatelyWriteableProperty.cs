// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    /// <summary>
    /// Get-only property that is made writeable for override purposes.
    /// </summary>
    internal class PrivatelyWriteableProperty : PropertyBuilder, IReplaceMember
    {
        private readonly IProperty _originalProperty;

        public MemberRef<IMember>? ReplacedMember => this._originalProperty.ToMemberRef<IMember>();

        public PrivatelyWriteableProperty( Advice advice, IProperty originalProperty, ITagReader tags ) : base(
            advice,
            originalProperty.DeclaringType,
            originalProperty.Name,
            true,
            true,
            true,
            false,
            tags )
        {
            Invariant.Assert( originalProperty.IsAutoPropertyOrField );
            Invariant.Assert( originalProperty.Writeability == Writeability.ConstructorOnly );

            this._originalProperty = originalProperty;
            this.Type = originalProperty.Type;
            this.Accessibility = originalProperty.Accessibility;
            this.IsStatic = originalProperty.IsStatic;

            this.SetMethod.AssertNotNull().Accessibility = Code.Accessibility.Private;

            foreach ( var attribute in originalProperty.Attributes )
            {
                this.AddAttribute( attribute.ToAttributeConstruction() );
            }
        }

        public override InsertPosition InsertPosition => this._originalProperty.ToInsertPosition();

        public override SyntaxTree TargetSyntaxTree
            => this._originalProperty switch
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
            if ( this._originalProperty is BuiltProperty builtProperty )
            {
                var propertyBuilder = builtProperty.PropertyBuilder;

                return propertyBuilder.GetInitializerExpressionOrMethod(
                    context,
                    this.Type,
                    propertyBuilder.InitializerExpression,
                    propertyBuilder.InitializerTemplate,
                    out initializerExpression,
                    out initializerMethod );
            }
            else if ( this._originalProperty is PromotedField promotedField )
            {
                return promotedField.GetInitializerExpressionOrMethod(
                    context,
                    this.Type,
                    promotedField.InitializerExpression,
                    promotedField.InitializerTemplate,
                    out initializerExpression,
                    out initializerMethod );
            }
            else
            {
                // For original code fields, copy the initializer syntax.
                var propertyDeclaration = (PropertyDeclarationSyntax) this._originalProperty.GetPrimaryDeclaration().AssertNotNull();

                if ( propertyDeclaration.Initializer != null )
                {
                    initializerExpression = propertyDeclaration.Initializer.Value;
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