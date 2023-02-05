// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal sealed class FieldOrPropertyInvoker : Invoker<IFieldOrProperty>, IFieldOrPropertyInvoker
    {
        public FieldOrPropertyInvoker( IFieldOrProperty fieldOrProperty, InvokerOptions options = default, object? target = null ) : base(
            fieldOrProperty,
            options,
            target ) { }

        private ExpressionSyntax CreatePropertyExpression( AspectReferenceTargetKind targetKind )
        {
            var receiverInfo = this.GetReceiverInfo();
            var receiverSyntax = this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext );

            var name = IdentifierName( this.Member.Name );

            ExpressionSyntax expression;

            if ( !receiverInfo.RequiresConditionalAccess )
            {
                expression = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiverSyntax, name );
            }
            else
            {
                expression = ConditionalAccessExpression( receiverSyntax, MemberBindingExpression( name ) );
            }

            // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this.Member.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( receiverInfo.AspectReferenceSpecification.WithTargetKind( targetKind ) );
            }

            return expression;
        }

        IType IHasType.Type => this.Member.Type;

        RefKind IHasType.RefKind => this.Member.RefKind;

        bool IExpression.IsAssignable => this.Member.IsAssignable;

        public object SetValue( object? value )
        {
            var propertyAccess = this.CreatePropertyExpression( AspectReferenceTargetKind.PropertySetAccessor );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Member.Compilation, this.GenerationContext ) );

            return new SyntaxUserExpression( expression, this.Member.Type );
        }

        public ref object? Value
            => ref RefHelper.Wrap(
                new SyntaxUserExpression(
                    this.CreatePropertyExpression( AspectReferenceTargetKind.PropertyGetAccessor ),
                    this.Member.Type,
                    isReferenceable: this.Member.DeclarationKind == DeclarationKind.Field,
                    isAssignable: this.Member.Writeability != Writeability.None ) );

        public IFieldOrPropertyInvoker With( InvokerOptions options ) => this.Options == options ? this : new FieldOrPropertyInvoker( this.Member, options );

        public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default )
            => this.Target == target && this.Options == options ? this : new FieldOrPropertyInvoker( this.Member, options, target );
    }
}