// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class FieldOrPropertyInvocation : IFieldOrPropertyInvocation
    {
        public IFieldOrProperty Member { get; }

        public FieldOrPropertyInvocation( IFieldOrProperty member )
        {
            this.Member = member;
        }

        public object Value
        {
            get => this.GetValue( this.Member.IsStatic ? null : new RuntimeExpression( ThisExpression(), this.Member.DeclaringType ) );
            set => throw new NotSupportedException();
        }

        protected virtual void AssertNoArgument() { }

        private ExpressionSyntax CreatePropertyExpression( RuntimeExpression? instance )
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

        public object GetValue( object? instance )
        {
            return new DynamicExpression( this.CreatePropertyExpression( RuntimeExpression.FromValue( instance ) ), this.Member.Type, this.Member is Field );
        }

        public object SetValue( object? instance, object? value )
        {
            var propertyAccess = this.CreatePropertyExpression( RuntimeExpression.FromValue( instance ) );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.Type, false );
        }

        public IPropertyInvocation Base => throw new NotImplementedException();
    }
}