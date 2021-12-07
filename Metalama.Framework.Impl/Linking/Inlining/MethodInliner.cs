// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Impl.Linking.Inlining
{
    /// <summary>
    /// Shared functionality for method inliners.
    /// </summary>
    internal abstract class MethodInliner : Inliner
    {
        protected static bool IsInlineableInvocation(
            SemanticModel semanticModel,
            IMethodSymbol contextMethod,
            InvocationExpressionSyntax invocationExpression )
        {
            return
                invocationExpression.ArgumentList.Arguments.Count == contextMethod.Parameters.Length
                && invocationExpression.ArgumentList.Arguments
                    .Select( ( x, i ) => (Argument: x.Expression, Index: i) )
                    .All( a => SymbolEqualityComparer.Default.Equals( semanticModel.GetSymbolInfo( a.Argument ).Symbol, contextMethod.Parameters[a.Index] ) );
        }

        public override bool IsValidForTargetSymbol( ISymbol symbol )
        {
            return symbol is IMethodSymbol { AssociatedSymbol: null, IsAsync: false } methodSymbol && !IteratorHelper.IsIterator( methodSymbol );
        }

        public override bool IsValidForContainingSymbol( ISymbol symbol )
        {
            return true;
        }
    }
}