// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    internal class EventInvoker : Invoker, IEventInvoker
    {
        public IEvent Member { get; }

        public EventInvoker( IEvent member, InvokerOrder order, InvokerOperator invokerOperator ) : base( member, order )
        {
            this.Member = member;

            if ( invokerOperator == InvokerOperator.Conditional )
            {
                throw new NotSupportedException( "Conditional access is not supported for events." );
            }
        }

        protected virtual void AssertNoArgument() { }

        private ExpressionSyntax CreateEventExpression( RuntimeExpression? instance, AspectReferenceTargetKind targetKind )
        {
            if ( this.Member.DeclaringType!.IsOpenGeneric )
            {
                throw GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember.CreateException( this.Member );
            }

            this.AssertNoArgument();

            return
                MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        this.Member.GetReceiverSyntax( instance ),
                        IdentifierName( this.Member.Name ) )
                    .WithAspectReferenceAnnotation( this.AspectReference.WithTargetKind( targetKind ) );
        }

        public object Add( object? instance, object? value )
        {
            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ), AspectReferenceTargetKind.EventAddAccessor );

            var expression = AssignmentExpression( SyntaxKind.AddAssignmentExpression, eventAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.EventType, false );
        }

        public object Remove( object? instance, object? value )
        {
            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ), AspectReferenceTargetKind.EventRemoveAccessor );

            var expression = AssignmentExpression( SyntaxKind.SubtractAssignmentExpression, eventAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.EventType, false );
        }

        public object? Raise( object? instance, params object?[] args )
        {
            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ), AspectReferenceTargetKind.EventRaiseAccessor );

            var arguments = this.Member.GetArguments( this.Member.Signature.Parameters, RuntimeExpression.FromValue( args ) );

            var expression = ConditionalAccessExpression(
                eventAccess,
                InvocationExpression( MemberBindingExpression( IdentifierName( "Invoke" ) ) ).AddArgumentListArguments( arguments ) );

            return new DynamicExpression(
                expression,
                this.Member.Signature.ReturnType,
                false );
        }

        public IEventInvoker Base => throw new NotImplementedException();
    }
}