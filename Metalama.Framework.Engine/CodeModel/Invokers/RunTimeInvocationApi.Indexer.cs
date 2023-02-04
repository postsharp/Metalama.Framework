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
    internal partial class IndexerInvoker : Invoker<IIndexer>, IIndexerInvoker
    {
        public IndexerInvoker( IIndexer indexer, InvokerOptions options = default ) : base( indexer, options ) { }

        public object GetValue( object? target, params object?[] args )
        {
            return new SyntaxUserExpression(
                this.CreateIndexerAccess(
                    target,
                    args ),
                this.Declaration.Type,
                isAssignable: this.Declaration.Writeability != Writeability.None );
        }

        public object SetValue( object? target, object value, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var propertyAccess = this.CreateIndexerAccess(
                target,
                args );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Declaration.Compilation, syntaxGenerationContext ) );

            return new SyntaxUserExpression( expression, this.Declaration.Type, isAssignable: true );
        }

        private ExpressionSyntax CreateIndexerAccess(
            object? target,
            object?[]? args )
        {
            var receiverInfo = this.GetReceiverInfo( this.Declaration, target );
            var receiverSyntax = this.Declaration.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext );
            var argExpressions = TypedExpressionSyntaxImpl.FromValues( args, this.Declaration.Compilation, this.GenerationContext ).AssertNotNull();

            var expression = ElementAccessExpression( receiverSyntax ).AddArgumentListArguments( argExpressions.SelectAsArray( e => Argument( e.Syntax ) ) );

            return expression;
        }
    }
}