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
        public IndexerInvoker( IIndexer indexer, InvokerOptions options = default, object? target = null ) : base( indexer, options, target ) { }

        public object GetValue( params object?[] args )
        {
            return new SyntaxUserExpression(
                this.CreateIndexerAccess( args ),
                this.Member.Type,
                isAssignable: this.Member.Writeability != Writeability.None );
        }

        public object SetValue( object value, params object?[] args )
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            var propertyAccess = this.CreateIndexerAccess( args );

            var expression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                propertyAccess,
                TypedExpressionSyntaxImpl.GetSyntaxFromValue( value, this.Member.Compilation, syntaxGenerationContext ) );

            return new SyntaxUserExpression( expression, this.Member.Type, isAssignable: true );
        }

        private ExpressionSyntax CreateIndexerAccess( object?[]? args )
        {
            var receiverInfo = this.GetReceiverInfo();
            var receiverSyntax = this.Member.GetReceiverSyntax( receiverInfo.TypedExpressionSyntax, this.GenerationContext );
            var argExpressions = TypedExpressionSyntaxImpl.FromValues( args, this.Member.Compilation, this.GenerationContext ).AssertNotNull();

            var expression = ElementAccessExpression( receiverSyntax ).AddArgumentListArguments( argExpressions.SelectAsArray( e => Argument( e.Syntax ) ) );

            return expression;
        }
    }
}