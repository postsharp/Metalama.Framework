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
    internal class IndexerInvoker : Invoker, IIndexerInvoker
    {
        private ExpressionSyntax CreateIndexerAccess( RuntimeExpression instance, RuntimeExpression[]? args, SyntaxGenerationContext generationContext )
        {
            if ( this.Indexer.DeclaringType.IsOpenGeneric )
            {
                throw new InvalidOperationException(
                    $"Cannot invoke the '{this.Indexer.ToDisplayString()}' event because the declaring type has unbound type parameters." );
            }

            var receiver = this.Indexer.GetReceiverSyntax( instance, generationContext );
            var arguments = this.Indexer.GetArguments( this.Indexer.Parameters, args );

            var expression = ElementAccessExpression( receiver ).AddArgumentListArguments( arguments );

            return expression;
        }

        public object GetValue( object? instance, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            return new UserExpression(
                this.CreateIndexerAccess(
                    RuntimeExpression.FromValue( instance, this.Compilation, syntaxGenerationContext ),
                    RuntimeExpression.FromValue( args, this.Compilation, syntaxGenerationContext ),
                    syntaxGenerationContext ),
                this.Indexer.Type,
                syntaxGenerationContext,
                isReferenceable: this.Indexer.Writeability != Writeability.None );
        }

        public object SetValue( object? instance, object value, params object?[] args )
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

            return new UserExpression( expression, this.Indexer.Type, syntaxGenerationContext );
        }

        public IndexerInvoker( IIndexer indexer, InvokerOrder order, InvokerOperator invokerOperator ) : base( indexer, order )
        {
            this.Indexer = indexer;
        }

        public IIndexer Indexer { get; }
    }
}