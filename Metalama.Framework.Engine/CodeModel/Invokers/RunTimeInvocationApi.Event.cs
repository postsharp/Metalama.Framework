// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal partial class RunTimeInvocationApi
    {
        public object Add( IEvent @event, object? instance, object? value )
        {
            var eventAccess = this.CreateEventExpression(
                @event,
                instance,
                AspectReferenceTargetKind.EventAddAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.AddAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, @event.Compilation, this._generationContext ) );

            return new SyntaxUserExpression( expression, @event.Type );
        }

        public object Remove( IEvent @event, object? instance, object? value )
        {
            var eventAccess = this.CreateEventExpression(
                @event,
                TypedExpressionSyntaxImpl.FromValue( instance, @event.Compilation, this._generationContext ),
                AspectReferenceTargetKind.EventRemoveAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.SubtractAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, @event.Compilation, this._generationContext ) );

            return new SyntaxUserExpression( expression, @event.Type );
        }

        public object Raise( IEvent @event, object? instance, params object?[] args )
        {
            var eventAccess = this.CreateEventExpression(
                @event,
                instance,
                AspectReferenceTargetKind.EventRaiseAccessor );

            var arguments = @event.GetArguments(
                @event.Signature.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, @event.Compilation, this._generationContext ),
                this._generationContext );

            var expression = ConditionalAccessExpression(
                eventAccess,
                InvocationExpression( MemberBindingExpression( IdentifierName( "Invoke" ) ) ).AddArgumentListArguments( arguments ) );

            return new SyntaxUserExpression(
                expression,
                @event.Signature.ReturnType );
        }

        private ExpressionSyntax CreateEventExpression(
            IEvent @event,
            object? target,
            AspectReferenceTargetKind targetKind )
        {
            var receiverInfo = this.GetReceiverInfo( @event, target );
            var receiverSyntax = @event.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this._generationContext );

            var expression = receiverInfo.RequiresConditionalAccess
                ? (ExpressionSyntax) ConditionalAccessExpression( receiverSyntax, MemberBindingExpression( IdentifierName( @event.Name ) ) )
                : MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    receiverSyntax,
                    IdentifierName( @event.Name ) );

            // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), @event.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
            }

            return expression;
        }
    }
}