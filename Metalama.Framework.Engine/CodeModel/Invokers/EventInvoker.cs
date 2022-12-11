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
    internal class EventInvoker : Invoker, IEventInvoker
    {
        private readonly IEvent _event;

        public EventInvoker( IEvent @event, InvokerOrder order, InvokerOperator invokerOperator ) : base( @event, order )
        {
            this._event = @event;

            if ( invokerOperator == InvokerOperator.Conditional )
            {
                throw new NotSupportedException( "Conditional access is not supported for events." );
            }
        }

        protected virtual void AssertNoArgument() { }

        private ExpressionSyntax CreateEventExpression(
            TypedExpressionSyntaxImpl instance,
            AspectReferenceTargetKind targetKind,
            SyntaxGenerationContext generationContext )
        {
            this.AssertNoArgument();

            var expression =
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this._event.GetReceiverSyntax( instance, generationContext ),
                    IdentifierName( this._event.Name ) );

            // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this._event.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( this.AspectReference.WithTargetKind( targetKind ) );
            }

            return expression;
        }

        public object Add( object? instance, object? value )
        {
            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var eventAccess = this.CreateEventExpression(
                TypedExpressionSyntaxImpl.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.EventAddAccessor,
                generationContext );

            var expression = AssignmentExpression(
                SyntaxKind.AddAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Compilation, generationContext ) );

            return new BuiltUserExpression( expression, this._event.Type );
        }

        public object Remove( object? instance, object? value )
        {
            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var eventAccess = this.CreateEventExpression(
                TypedExpressionSyntaxImpl.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.EventRemoveAccessor,
                generationContext );

            var expression = AssignmentExpression(
                SyntaxKind.SubtractAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Compilation, generationContext ) );

            return new BuiltUserExpression( expression, this._event.Type );
        }

        public object Raise( object? instance, params object?[] args )
        {
            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var eventAccess = this.CreateEventExpression(
                TypedExpressionSyntaxImpl.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.EventRaiseAccessor,
                generationContext );

            var arguments = this._event.GetArguments(
                this._event.Signature.Parameters,
                TypedExpressionSyntaxImpl.FromValue( args, this.Compilation, generationContext ),
                generationContext );

            var expression = ConditionalAccessExpression(
                eventAccess,
                InvocationExpression( MemberBindingExpression( IdentifierName( "Invoke" ) ) ).AddArgumentListArguments( arguments ) );

            return new BuiltUserExpression(
                expression,
                this._event.Signature.ReturnType );
        }
    }
}