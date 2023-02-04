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
    internal class EventInvoker : Invoker<IEvent>, IEventInvoker
    {
        public EventInvoker( IEvent @event, InvokerOptions options = default ) : base( @event, options ) { }

        public object Add( object? instance, object? value )
        {
            var eventAccess = this.CreateEventExpression(
                instance,
                AspectReferenceTargetKind.EventAddAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.AddAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Declaration.Compilation, this.GenerationContext ) );

            return new SyntaxUserExpression( expression, this.Declaration.Type );
        }

        public object Remove( object? instance, object? value )
        {
            var eventAccess = this.CreateEventExpression(
                TypedExpressionSyntaxImpl.FromValue( instance, this.Declaration.Compilation, this.GenerationContext ),
                AspectReferenceTargetKind.EventRemoveAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.SubtractAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Declaration.Compilation, this.GenerationContext ) );

            return new SyntaxUserExpression( expression, this.Declaration.Type );
        }

        public object Raise( object? instance, params object?[] args )
        {
            var eventAccess = this.CreateEventExpression(
                instance,
                AspectReferenceTargetKind.EventRaiseAccessor );

            var arguments = this.Declaration.GetArguments(
                this.Declaration.Signature.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, this.Declaration.Compilation, this.GenerationContext ),
                this.GenerationContext );

            var expression = ConditionalAccessExpression(
                eventAccess,
                InvocationExpression( MemberBindingExpression( IdentifierName( "Invoke" ) ) ).AddArgumentListArguments( arguments ) );

            return new SyntaxUserExpression(
                expression,
                this.Declaration.Signature.ReturnType );
        }

        private ExpressionSyntax CreateEventExpression(
            object? target,
            AspectReferenceTargetKind targetKind )
        {
            var receiverInfo = this.GetReceiverInfo( this.Declaration, target );
            var receiverSyntax = this.Declaration.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext );

            var expression = receiverInfo.RequiresConditionalAccess
                ? (ExpressionSyntax) ConditionalAccessExpression( receiverSyntax, MemberBindingExpression( IdentifierName( this.Declaration.Name ) ) )
                : MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    receiverSyntax,
                    IdentifierName( this.Declaration.Name ) );

            // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this.Declaration.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
            }

            return expression;
        }
    }
}