// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class PropertyInvoker : FieldOrPropertyInvoker, IPropertyInvoker
    {
        protected IProperty Property => (IProperty) this.Member;

        protected override void AssertNoArgument()
        {
            this.Member.CheckArguments( this.Property.Parameters, null );
        }

        private ExpressionSyntax CreateIndexerAccess( RuntimeExpression? instance, RuntimeExpression[]? args )
        {
            if ( this.Member.DeclaringType!.IsOpenGeneric )
            {
                throw GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember.CreateException( this.Member );
            }

            var receiver = this.Member.GetReceiverSyntax( instance );
            var arguments = this.Member.GetArguments( this.Property.Parameters, args );

            var expression = ElementAccessExpression( receiver ).AddArgumentListArguments( arguments );

            return expression;
        }

        public object GetIndexerValue( object? instance, params object[] args )
        {
            return new DynamicExpression(
                this.CreateIndexerAccess(
                    RuntimeExpression.FromValue( instance ),
                    RuntimeExpression.FromValue( args ) ),
                this.Member.Type,
                false );
        }

        public object SetIndexerValue( object? instance, object value, params object[] args )
        {
            var propertyAccess = this.CreateIndexerAccess( RuntimeExpression.FromValue( instance ), RuntimeExpression.FromValue( args ) );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, RuntimeExpression.GetSyntaxFromValue( value ) );

            return new DynamicExpression( expression, this.Member.Type, false );
        }

        public PropertyInvoker( IProperty member, LinkingOrder order ) : base( member, order ) { }
    }
}