// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
    internal class FieldOrPropertyInvoker : Invoker, IFieldOrPropertyInvoker
    {
        private readonly InvokerOperator _invokerOperator;

        protected IFieldOrProperty Member { get; }

        public FieldOrPropertyInvoker( IFieldOrProperty member, InvokerOrder linkerOrder, InvokerOperator invokerOperator ) : base( member, linkerOrder )
        {
            this._invokerOperator = invokerOperator;
            this.Member = member;
        }

        protected virtual void AssertNoArgument() { }

        private ExpressionSyntax CreatePropertyExpression(
            RuntimeExpression instance,
            AspectReferenceTargetKind targetKind,
            SyntaxGenerationContext generationContext )
        {
            if ( this.Member.DeclaringType.IsOpenGeneric )
            {
                throw new InvalidOperationException(
                    $"Cannot invoke '{this.Member.ToDisplayString()}' because the declaring type has unbound type parameters." );
            }

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

            return new UserExpression(
                this.CreatePropertyExpression(
                    RuntimeExpression.FromValue( instance, this.Compilation, generationContext ),
                    AspectReferenceTargetKind.PropertyGetAccessor,
                    generationContext ),
                this._invokerOperator == InvokerOperator.Default ? this.Member.Type : this.Member.Type.ConstructNullable(),
                generationContext,
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
                RuntimeExpression.FromValue( instance, this.Compilation, generationContext ),
                AspectReferenceTargetKind.PropertySetAccessor,
                generationContext );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                RuntimeExpression.GetSyntaxFromValue( value, this.Compilation, generationContext ) );

            return new UserExpression( expression, this.Member.Type, generationContext );
        }
    }
}