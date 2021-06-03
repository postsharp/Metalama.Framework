// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Invokers
{
    internal class FieldOrPropertyInvoker : Invoker, IFieldOrPropertyInvoker
    {
        public IFieldOrProperty Member { get; }

        public FieldOrPropertyInvoker( IFieldOrProperty member, InvokerOrder linkerOrder ) : base( member, linkerOrder )
        {
            this.Member = member;
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
            // TODO: Use LinkerAnnotation.
            return new DynamicExpression( this.CreatePropertyExpression( RuntimeExpression.FromValue( instance ) ), this.Member.Type, this.Member is Field );
        }

        public object SetValue( object? instance, object? value )
        {
            // TODO: Use LinkerAnnotation.

            var propertyAccess = this.CreatePropertyExpression( RuntimeExpression.FromValue( instance ) );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.Type, false );
        }
    }
}