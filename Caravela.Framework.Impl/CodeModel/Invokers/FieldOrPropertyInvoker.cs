﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
    internal class FieldOrPropertyInvoker : Invoker, IFieldOrPropertyInvoker
    {
        private readonly InvokerOperator _invokerOperator;

        public IFieldOrProperty Member { get; }

        public FieldOrPropertyInvoker( IFieldOrProperty member, InvokerOrder linkerOrder, InvokerOperator invokerOperator ) : base( member, linkerOrder )
        {
            this._invokerOperator = invokerOperator;
            this.Member = member;
        }

        protected virtual void AssertNoArgument() { }

        private ExpressionSyntax CreatePropertyExpression( RuntimeExpression? instance, AspectReferenceTargetKind targetKind )
        {
            if ( this.Member.DeclaringType!.IsOpenGeneric )
            {
                throw GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember.CreateException( this.Member );
            }

            this.AssertNoArgument();

            var receiver = this.Member.GetReceiverSyntax( instance );
            var name = IdentifierName( this.Member.Name );

            if ( this._invokerOperator == InvokerOperator.Default )
            {
                return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name )
                    .WithAspectReferenceAnnotation( this.AspectReference.WithTargetKind( targetKind ) );
            }
            else
            {
                return ConditionalAccessExpression( receiver, MemberBindingExpression( name ) )
                    .WithAspectReferenceAnnotation( this.AspectReference.WithTargetKind( targetKind ) );
            }
        }

        public object GetValue( object? instance )
            => new DynamicExpression(
                this.CreatePropertyExpression( RuntimeExpression.FromValue( instance, this.Compilation ), AspectReferenceTargetKind.PropertyGetAccessor ),
                this._invokerOperator == InvokerOperator.Default ? this.Member.Type : this.Member.Type.MakeNullable(),
                this.Member is Field );

        public object SetValue( object? instance, object? value )
        {
            if ( this._invokerOperator == InvokerOperator.Conditional )
            {
                throw new NotSupportedException( "Conditional access is not supported for SetValue." );
            }

            var propertyAccess = this.CreatePropertyExpression( RuntimeExpression.FromValue( instance, this.Compilation ), AspectReferenceTargetKind.PropertySetAccessor );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, RuntimeExpression.GetSyntaxFromValue( value, this.Compilation ) );

            return new DynamicExpression( expression, this.Member.Type, false );
        }
    }
}