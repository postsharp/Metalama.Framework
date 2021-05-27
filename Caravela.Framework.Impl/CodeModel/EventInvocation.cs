﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class EventInvocation : IEventInvocation
    {
        public IEvent Member { get; }

        public EventInvocation( IEvent member )
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
            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ) );

            var expression = AssignmentExpression( SyntaxKind.AddAssignmentExpression, eventAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.EventType, false );
        }

        public object RemoveDelegate( object? instance, object? value )
        {
            var eventAccess = this.CreateEventExpression( RuntimeExpression.FromValue( instance ) );

            var expression = AssignmentExpression( SyntaxKind.SubtractAssignmentExpression, eventAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.EventType, false );
        }

        public IEventInvocation Base => throw new NotImplementedException();
    }
}