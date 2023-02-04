// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Invokers
{
    internal partial class RunTimeInvocationApi
    {
        public object GetValue( IIndexer indexer, object? target, params object?[] args )
        {
            return new SyntaxUserExpression(
                this.CreateIndexerAccess(
                    indexer,
                    target,
                    args ),
                indexer.Type,
                isAssignable: indexer.Writeability != Writeability.None );
        }

        public object SetValue( IIndexer indexer, object? target, object value, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var propertyAccess = this.CreateIndexerAccess(
                indexer,
                target,
                args );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, indexer.Compilation, syntaxGenerationContext ) );

            return new SyntaxUserExpression( expression, indexer.Type, isAssignable: true );
        }

        private ExpressionSyntax CreateIndexerAccess(
            IIndexer indexer,
            object? target,
            object?[]? args )
        {
            var receiverInfo = this.GetReceiverInfo( indexer, target );
            var receiverSyntax = indexer.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this._generationContext );
            var argExpressions = TypedExpressionSyntaxImpl.FromValues( args, indexer.Compilation, this._generationContext ).AssertNotNull();

            var expression = ElementAccessExpression( receiverSyntax ).AddArgumentListArguments( argExpressions.SelectAsArray( e => Argument( e.Syntax ) ) );

            return expression;
        }
    }
}