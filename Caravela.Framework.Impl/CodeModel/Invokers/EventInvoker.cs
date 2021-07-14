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

        public EventInvoker( IEvent member, InvokerOrder order ) : base( member, order )
        {
            this.Member = member;
        }

        protected virtual void AssertNoArgument() { }

        private ExpressionSyntax CreateEventExpression( RuntimeExpression? instance )
        {
            if ( this.Member.DeclaringType!.IsOpenGeneric )
            {
                throw GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember.CreateException( this.Member );
            }

            this.AssertNoArgument();

            return MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                this.Member.GetReceiverSyntax( instance ),
                IdentifierName( this.Member.Name ) );
        }

        public object AddDelegate( object? instance, object? value )
        {
            // TODO: Use LinkerAnnotation.

            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ) );

            var expression = AssignmentExpression( SyntaxKind.AddAssignmentExpression, eventAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.EventType, false );
        }

        public object RemoveDelegate( object? instance, object? value )
        {
            // TODO: Use LinkerAnnotation.

            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ) );

            var expression = AssignmentExpression( SyntaxKind.SubtractAssignmentExpression, eventAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.EventType, false );
        }

        public object? Raise( object? instance, params object?[] args )
        {
            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ) );

            var arguments = this.Member.GetArguments( this.Member.Signature.Parameters, RuntimeExpression.FromValue( args ) );

            var expression = InvocationExpression( ConditionalAccessExpression( eventAccess, MemberBindingExpression( IdentifierName( "Invoke" ) ) ) )
                .AddArgumentListArguments( arguments );

            return new DynamicExpression(
                expression,
                this.Member.Signature.ReturnType,
                false );
        }

        public IEventInvoker Base => throw new NotImplementedException();
    }
}