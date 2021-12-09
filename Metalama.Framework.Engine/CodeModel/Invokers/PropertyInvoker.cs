// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal class PropertyInvoker : FieldOrPropertyInvoker, IPropertyInvoker
    {
        protected IProperty Property => (IProperty) this.Member;

        protected override void AssertNoArgument() => this.Member.CheckArguments( this.Property.Parameters, null );

        private ExpressionSyntax CreateIndexerAccess( RuntimeExpression instance, RuntimeExpression[]? args, SyntaxGenerationContext generationContext )
        {
            if ( this.Member.DeclaringType.IsOpenGeneric )
            {
                throw new InvalidOperationException(
                    $"Cannot invoke the '{this.Property.ToDisplayString()}' event because the declaring type has unbound type parameters." );
            }

            var receiver = this.Member.GetReceiverSyntax( instance, generationContext );
            var arguments = this.Member.GetArguments( this.Property.Parameters, args );

            var expression = ElementAccessExpression( receiver ).AddArgumentListArguments( arguments );

            return expression;
        }

        public object GetIndexerValue( object? instance, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            return new UserExpression(
                this.CreateIndexerAccess(
                    RuntimeExpression.FromValue( instance, this.Compilation, syntaxGenerationContext ),
                    RuntimeExpression.FromValue( args, this.Compilation, syntaxGenerationContext ),
                    syntaxGenerationContext ),
                this.Member.Type,
                syntaxGenerationContext,
                isReferenceable: this.Member.Writeability != Writeability.None );
        }

        public object SetIndexerValue( object? instance, object value, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var propertyAccess = this.CreateIndexerAccess(
                RuntimeExpression.FromValue( instance, this.Compilation, syntaxGenerationContext ),
                RuntimeExpression.FromValue( args, this.Compilation, syntaxGenerationContext ),
                syntaxGenerationContext );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                RuntimeExpression.GetSyntaxFromValue( value, this.Compilation, syntaxGenerationContext ) );

            return new UserExpression( expression, this.Member.Type, syntaxGenerationContext );
        }

        public PropertyInvoker( IProperty member, InvokerOrder order, InvokerOperator invokerOperator ) : base( member, order, invokerOperator ) { }
    }
}