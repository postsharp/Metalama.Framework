// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Caravela.Framework.Impl.Linking.Inlining
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
                invocationExpression.ArgumentList.Arguments.Count != contextMethod.Parameters.Length
                && invocationExpression.ArgumentList.Arguments
                    .Select( ( x, i ) => (Argument: x, Index: i) )
                    .Any( a => !SymbolEqualityComparer.Default.Equals( semanticModel.GetSymbolInfo( a.Argument ).Symbol, contextMethod.Parameters[a.Index] ) );
        }
    }
}