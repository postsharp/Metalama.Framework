using System;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel
{
    internal readonly struct PropertyInvocation<TProperty> : IPropertyInvocation
        where TProperty : CodeElement, IProperty
    {
        private readonly TProperty _property;

        public PropertyInvocation( TProperty property )
        {
            this._property = property;
        }

        public object Value
        {
            get => this.GetValue( this._property.IsStatic ? null : new CurrentTypeOrInstanceDynamic( true, this._property.DeclaringType ).CreateExpression() );
            set => throw new NotSupportedException();
        }

        private ExpressionSyntax CreatePropertyExpression( RuntimeExpression? instance )
        {
            if ( this._property.DeclaringType!.IsOpenGeneric )
            {
                throw new InvalidUserCodeException( GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember, this._property );
            }

            this._property.CheckArguments( this._property.Parameters, null );

            return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, this._property.GetReceiverSyntax( instance ), IdentifierName( this._property.Name ) );
        }

        public object GetValue( object? instance )
        {
            return new DynamicMember( this.CreatePropertyExpression( RuntimeExpression.FromDynamic( instance ) ), this._property.Type, this._property is Field );
        }

        public object SetValue( object? instance, object? value )
        {
            var propertyAccess = this.CreatePropertyExpression( RuntimeExpression.FromDynamic( instance ) );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, RuntimeExpression.GetSyntaxFromDynamic( value ) );

            return new DynamicMember( expression, this._property.Type, false );
        }

        private ExpressionSyntax CreateIndexerAccess( RuntimeExpression? instance, RuntimeExpression[]? args )
        {
            if ( this._property.DeclaringType!.IsOpenGeneric )
            {
                throw new InvalidUserCodeException( GeneralDiagnosticDescriptors.CannotAccessOpenGenericMember, this._property );
            }

            var receiver = this._property.GetReceiverSyntax( instance );
            var arguments = this._property.GetArguments( this._property.Parameters, args );

            var expression = ElementAccessExpression( receiver ).AddArgumentListArguments( arguments );
            return expression;
        }

        public object GetIndexerValue( object? instance, params object[] args )
        {
            return new DynamicMember( this.CreateIndexerAccess( RuntimeExpression.FromDynamic( instance ), RuntimeExpression.FromDynamic( args ) ), this._property.Type, false );
        }

        public object SetIndexerValue( object? instance, object value, params object[] args )
        {
            var propertyAccess = this.CreateIndexerAccess( RuntimeExpression.FromDynamic( instance ), RuntimeExpression.FromDynamic( args ) );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, RuntimeExpression.GetSyntaxFromDynamic( value ) );

            return new DynamicMember( expression, this._property.Type, false );
        }

        public bool HasBase => true;

        public IPropertyInvocation Base => throw new NotImplementedException();
    }
}
