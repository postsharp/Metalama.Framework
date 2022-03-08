﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
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
            RuntimeExpression instance,
            AspectReferenceTargetKind targetKind,
            SyntaxGenerationContext generationContext )
        {
            if ( this._event.DeclaringType.IsOpenGeneric )
            {
                throw new InvalidOperationException(
                    $"Cannot invoke the '{this._event.ToDisplayString()}' event because the declaring type has unbound type parameters." );
            }

            this.AssertNoArgument();

            var expression = 
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    this._event.GetReceiverSyntax( instance, generationContext ),
                    IdentifierName( this._event.Name ) );

            // Only create an aspect reference when the target type is the target of the template.
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
                RuntimeExpression.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.EventAddAccessor,
                generationContext );

            var expression = AssignmentExpression(
                SyntaxKind.AddAssignmentExpression,
                eventAccess,
                RuntimeExpression.GetSyntaxFromValue( value, this.Compilation, generationContext ) );

            return new UserExpression( expression, this._event.Type, generationContext );
        }

        public object Remove( object? instance, object? value )
        {
            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var eventAccess = this.CreateEventExpression(
                RuntimeExpression.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.EventRemoveAccessor,
                generationContext );

            var expression = AssignmentExpression(
                SyntaxKind.SubtractAssignmentExpression,
                eventAccess,
                RuntimeExpression.GetSyntaxFromValue( value, this.Compilation, generationContext ) );

            return new UserExpression( expression, this._event.Type, generationContext );
        }

        public object? Raise( object? instance, params object?[] args )
        {
            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var eventAccess = this.CreateEventExpression(
                RuntimeExpression.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.EventRaiseAccessor,
                generationContext );

            var arguments = this._event.GetArguments(
                this._event.Signature.Parameters,
                RuntimeExpression.FromValue( args, this.Compilation, generationContext ) );

            var expression = ConditionalAccessExpression(
                eventAccess,
                InvocationExpression( MemberBindingExpression( IdentifierName( "Invoke" ) ) ).AddArgumentListArguments( arguments ) );

            return new UserExpression(
                expression,
                this._event.Signature.ReturnType,
                generationContext );
        }
    }
}