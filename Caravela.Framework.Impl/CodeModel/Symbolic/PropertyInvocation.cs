using System;
using Caravela.Framework.Aspects;
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
            get => this.GetValue(
                this._property.IsStatic ?
                    new RuntimeExpression( null!, isNull: true ) :
                    ((IDynamicMetaMember) (object) TemplateContext.target.This).CreateExpression() );
            set => throw new NotImplementedException();
        }

        private ExpressionSyntax CreatePropertyAccess( object instance )
        {
            if ( this._property.DeclaringType!.IsOpenGeneric )
            {
                throw new CaravelaException( GeneralDiagnosticDescriptors.CantAccessOpenGenericMember, this._property );
            }

            this._property.CheckArguments( this._property.Parameters, Array.Empty<IParameter>() );

            return MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, this._property.GetReceiverSyntax( instance ), IdentifierName( this._property.Name ) );
        }

        public object GetValue( object? instance ) => new DynamicMetaMember( this.CreatePropertyAccess( instance! ), this._property.Type );

        public object SetValue( object? instance, object value )
        {
            var propertyAccess = this.CreatePropertyAccess( instance! );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, ((RuntimeExpression) value).Syntax );

            return new DynamicMetaMember( expression, this._property.Type );
        }

        private ExpressionSyntax CreateIndexerAccess( object instance, object[] args )
        {
            if ( this._property.DeclaringType!.IsOpenGeneric )
            {
                throw new CaravelaException( GeneralDiagnosticDescriptors.CantAccessOpenGenericMember, this._property );
            }

            var receiver = this._property.GetReceiverSyntax( instance );
            var arguments = this._property.GetArguments( this._property.Parameters, args );

            var expression = ElementAccessExpression( receiver ).AddArgumentListArguments( arguments );
            return expression;
        }

        public object GetIndexerValue( object? instance, params object[] args ) => new DynamicMetaMember( this.CreateIndexerAccess( instance!, args ), this._property.Type );

        public object SetIndexerValue( object? instance, object value, params object[] args )
        {
            var propertyAccess = this.CreateIndexerAccess( instance!, args );

            var expression = AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, propertyAccess, ((RuntimeExpression) value).Syntax );

            return new DynamicMetaMember( expression, this._property.Type );
        }

        public bool HasBase => true;

        public IPropertyInvocation Base => throw new NotImplementedException();
    }
}
