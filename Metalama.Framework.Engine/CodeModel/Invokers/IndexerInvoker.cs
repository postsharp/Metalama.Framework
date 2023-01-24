// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal sealed class IndexerInvoker : Invoker, IIndexerInvoker
    {
        private readonly IIndexer _indexer;

        private ExpressionSyntax CreateIndexerAccess(
            TypedExpressionSyntaxImpl instance,
            TypedExpressionSyntaxImpl[]? args,
            SyntaxGenerationContext generationContext )
        {
            var receiver = this._indexer.GetReceiverSyntax( instance, generationContext );
            var arguments = this._indexer.GetArguments( this._indexer.Parameters, args, generationContext );

            var expression = ElementAccessExpression( receiver ).AddArgumentListArguments( arguments );

            return expression;
        }

        public object GetValue( object? instance, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            return new SyntaxUserExpression(
                this.CreateIndexerAccess(
                    TypedExpressionSyntaxImpl.FromValue( instance, this.Compilation, syntaxGenerationContext ),
                    TypedExpressionSyntaxImpl.FromValue( args, this.Compilation, syntaxGenerationContext ),
                    syntaxGenerationContext ),
                this._indexer.Type,
                isReferenceable: this._indexer.Writeability != Writeability.None );
        }

        public object SetValue( object? instance, object value, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var propertyAccess = this.CreateIndexerAccess(
                TypedExpressionSyntaxImpl.FromValue( instance, this.Compilation, syntaxGenerationContext ),
                TypedExpressionSyntaxImpl.FromValue( args, this.Compilation, syntaxGenerationContext ),
                syntaxGenerationContext );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Compilation, syntaxGenerationContext ) );

            return new SyntaxUserExpression( expression, this._indexer.Type );
        }

        public IndexerInvoker( IIndexer indexer, InvokerOrder order ) : base( indexer, order )
        {
            this._indexer = indexer;
        }
    }
}