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
    internal partial class FieldOrPropertyInvoker : Invoker<IFieldOrProperty>, IFieldOrPropertyInvoker
    {
        public FieldOrPropertyInvoker( IFieldOrProperty fieldOrProperty, InvokerOptions options = default) : base( fieldOrProperty, options ) { }

        public object GetValue( object? instance )
        {
            return new SyntaxUserExpression(
                this.CreatePropertyExpression(
                    instance,
                    AspectReferenceTargetKind.PropertyGetAccessor ),
                this.Declaration.Type,
                isReferenceable: this.Declaration.DeclarationKind == DeclarationKind.Field,
                isAssignable: this.Declaration.Writeability != Writeability.None );
        }

        public object SetValue( object? instance, object? value )
        {
            var propertyAccess = this.CreatePropertyExpression(
                instance,
                AspectReferenceTargetKind.PropertySetAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Declaration.Compilation, this.GenerationContext ) );

            return new SyntaxUserExpression( expression, this.Declaration.Type );
        }

        private ExpressionSyntax CreatePropertyExpression(
            object? target,
            AspectReferenceTargetKind targetKind )
        {
            var receiverInfo = this.GetReceiverInfo( this.Declaration, target );
            var receiverSyntax = this.Declaration.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext );

            var name = IdentifierName( this.Declaration.Name );

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
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this.Declaration.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
            }

            return expression;
        }
    }
}