// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal sealed class EventInvoker : Invoker<IEvent>, IEventInvoker
    {
        public EventInvoker( IEvent @event, InvokerOptions? options = default, object? target = null ) : base( @event, options, target ) { }

        public object Add( object? value )
        {
            var eventAccess = this.CreateEventExpression( AspectReferenceTargetKind.EventAddAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.AddAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, CurrentSerializationContext ) );

            return new SyntaxUserExpression( expression, this.Member.Type );
        }

        public object Remove( object? value )
        {
            var eventAccess = this.CreateEventExpression( AspectReferenceTargetKind.EventRemoveAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.SubtractAssignmentExpression,
                eventAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, CurrentSerializationContext ) );

            return new SyntaxUserExpression( expression, this.Member.Type );
        }

        public object Raise( params object?[] args )
        {
            var eventAccess = this.CreateEventExpression( AspectReferenceTargetKind.EventRaiseAccessor );

            var arguments = this.Member.GetArguments(
                this.Member.Signature.Parameters,
                TypedExpressionSyntaxImpl.FromValues( args, CurrentSerializationContext ),
                CurrentGenerationContext );

            var expression = ConditionalAccessExpression(
                eventAccess,
                InvocationExpression( MemberBindingExpression( IdentifierName( "Invoke" ) ) ).AddArgumentListArguments( arguments ) );

            return new SyntaxUserExpression(
                expression,
                this.Member.Signature.ReturnType );
        }

        public IEventInvoker With( InvokerOptions options ) => this.Options == options ? this : new EventInvoker( this.Member, options );

        public IEventInvoker With( object? target, InvokerOptions options = default )
            => this.Target == target && this.Options == options ? this : new EventInvoker( this.Member, options, target );

        private ExpressionSyntax CreateEventExpression( AspectReferenceTargetKind targetKind )
        {
            this.CheckInvocationOptionsAndTarget();

            var receiverInfo = this.GetReceiverInfo();
            var name = IdentifierName( this.GetCleanTargetMemberName() );
            
            var receiverSyntax = this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, CurrentGenerationContext );

            var expression = receiverInfo.RequiresConditionalAccess
                ? (ExpressionSyntax) ConditionalAccessExpression( receiverSyntax, MemberBindingExpression( name ) )
                : MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    receiverSyntax,
                    name );

            // Only create an aspect reference when the declaring type of the invoked declaration is ancestor of the target of the template (or it's declaring type).
            if ( GetTargetType()?.Is( this.Member.DeclaringType ) ?? false )
            {
                expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
            }

            return expression;
        }
    }
}