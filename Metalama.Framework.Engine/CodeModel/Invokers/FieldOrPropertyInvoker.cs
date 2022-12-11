// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal class FieldOrPropertyInvoker : Invoker, IFieldOrPropertyInvoker
    {
        private readonly InvokerOperator _invokerOperator;

        protected IFieldOrProperty Member { get; }

        public FieldOrPropertyInvoker( IFieldOrProperty member, InvokerOrder linkerOrder, InvokerOperator invokerOperator ) : base( member, linkerOrder )
        {
            if ( member is { DeclarationKind: DeclarationKind.Field, IsImplicitlyDeclared: true } )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(member),
                    MetalamaStringFormatter.Format( $"Cannot create an invoker for '{member}' because it is an implicitly declared field." ) );
            }

            this._invokerOperator = invokerOperator;
            this.Member = member;
        }

        protected virtual void AssertNoArgument() { }

        private ExpressionSyntax CreatePropertyExpression(
            TypedExpressionSyntaxImpl instance,
            AspectReferenceTargetKind targetKind,
            SyntaxGenerationContext generationContext )
        {
            this.AssertNoArgument();

            var receiver = this.Member.GetReceiverSyntax( instance, generationContext );
            var name = IdentifierName( this.Member.Name );

            ExpressionSyntax expression;

            if ( this._invokerOperator == InvokerOperator.Default )
            {
                expression = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, name );
            }
            else
            {
                expression = ConditionalAccessExpression( receiver, MemberBindingExpression( name ) );
            }

            // Only create an aspect reference when the declaring type of the invoked declaration is the target of the template (or it's declaring type).
            if ( SymbolEqualityComparer.Default.Equals( GetTargetTypeSymbol(), this.Member.DeclaringType.GetSymbol().OriginalDefinition ) )
            {
                expression = expression.WithAspectReferenceAnnotation( this.AspectReference.WithTargetKind( targetKind ) );
            }

            return expression;
        }

        public object GetValue( object? instance )
        {
            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            return new BuiltUserExpression(
                this.CreatePropertyExpression(
                    TypedExpressionSyntaxImpl.FromValue( instance, this.Compilation, generationContext ),
                    AspectReferenceTargetKind.PropertyGetAccessor,
                    generationContext ),
                this._invokerOperator == InvokerOperator.Default ? this.Member.Type : this.Member.Type.ToNullableType(),
                isReferenceable: this.Member is Field,
                isAssignable: this.Member.Writeability != Writeability.None );
        }

        public object SetValue( object? instance, object? value )
        {
            if ( this._invokerOperator == InvokerOperator.Conditional )
            {
                throw new NotSupportedException( "Conditional access is not supported for SetValue." );
            }

            var generationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var propertyAccess = this.CreatePropertyExpression(
                TypedExpressionSyntaxImpl.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.PropertySetAccessor,
                generationContext );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Compilation, generationContext ) );

            return new BuiltUserExpression( expression, this.Member.Type );
        }
    }
}