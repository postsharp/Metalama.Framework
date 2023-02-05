﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal class EventInvoker : Invoker<IEvent>, IEventInvoker
    {
        public EventInvoker( IEvent @event, InvokerOptions options = default, object? target = null ) : base( @event, options, target ) { }

        public object Add( object? value )
        {
            var eventAccess = this.CreateEventExpression( AspectReferenceTargetKind.EventAddAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.AddAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Member.Compilation, this.GenerationContext ) );

            return new SyntaxUserExpression( expression, this.Member.Type );
        }

        public object Remove( object? value )
        {
            var eventAccess = this.CreateEventExpression( AspectReferenceTargetKind.EventRemoveAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.SubtractAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Member.Compilation, this.GenerationContext ) );

            return new SyntaxUserExpression( expression, this.Member.Type );
        }

        public object Raise( params object?[] args )
        {
            var eventAccess = this.CreateEventExpression( AspectReferenceTargetKind.EventRaiseAccessor );

            var arguments = this.Member.GetArguments(
                this.Member.Signature.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, this.Member.Compilation, this.GenerationContext ),
                this.GenerationContext );

            var expression = ConditionalAccessExpression(
                eventAccess,
                InvocationExpression( MemberBindingExpression( IdentifierName( "Invoke" ) ) ).AddArgumentListArguments( arguments ) );

            return new SyntaxUserExpression(
                expression,
                this.Member.Signature.ReturnType );
        }

        private ExpressionSyntax CreateEventExpression( AspectReferenceTargetKind targetKind )
        {
            var receiverInfo = this.GetReceiverInfo();
            var receiverSyntax = this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext );

            var expression = receiverInfo.RequiresConditionalAccess
                ? (ExpressionSyntax) ConditionalAccessExpression( receiverSyntax, MemberBindingExpression( IdentifierName( this.Member.Name ) ) )
                : MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    receiverSyntax,
                    IdentifierName( this.Member.Name ) );

            // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this.Member.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
            }

            return expression;
        }
    }
}