// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal partial class RunTimeInvocationApi
    {
        public object GetValue( IFieldOrProperty fieldOrProperty, object? instance )
        {
            return new SyntaxUserExpression(
                this.CreatePropertyExpression(
                    fieldOrProperty,
                    instance,
                    AspectReferenceTargetKind.PropertyGetAccessor ),
                fieldOrProperty.Type,
                isReferenceable: fieldOrProperty.DeclarationKind == DeclarationKind.Field,
                isAssignable: fieldOrProperty.Writeability != Writeability.None );
        }

        public object SetValue( IFieldOrProperty fieldOrProperty, object? instance, object? value )
        {
            var propertyAccess = this.CreatePropertyExpression(
                fieldOrProperty,
                instance,
                AspectReferenceTargetKind.PropertySetAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, fieldOrProperty.Compilation, this._generationContext ) );

            return new SyntaxUserExpression( expression, fieldOrProperty.Type );
        }

        private ExpressionSyntax CreatePropertyExpression(
            IFieldOrProperty fieldOrProperty,
            object? target,
            AspectReferenceTargetKind targetKind )
        {
            var receiverInfo = this.GetReceiverInfo( fieldOrProperty, target );
            var receiverSyntax = fieldOrProperty.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this._generationContext );

            var name = IdentifierName( fieldOrProperty.Name );

            ExpressionSyntax expression;

            if ( !receiverInfo.RequiresConditionalAccess )
            {
                expression = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiverSyntax, name );
            }
            else
            {
                expression = ConditionalAccessExpression( receiverSyntax, MemberBindingExpression( name ) );
            }

            // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), fieldOrProperty.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
            }

            return expression;
        }
    }
}