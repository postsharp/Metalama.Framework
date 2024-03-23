// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Linking.Inlining;

/// <summary>
/// Shared functionality for method inliners.
/// </summary>
internal abstract class MethodInliner : Inliner
{
    protected static bool IsInlineableInvocation(
        SemanticModel semanticModel,
        ResolvedAspectReference aspectReference,
        InvocationExpressionSyntax invocationExpression )
        => aspectReference.IsImplicitlyInlineableInvocation
           || (invocationExpression.ArgumentList.Arguments.Count == aspectReference.ContainingSemantic.Symbol.Parameters.Length
               && invocationExpression.ArgumentList.Arguments
                   .Select( ( x, i ) => (Argument: x.Expression, Index: i) )
                   .All(
                       a => SymbolEqualityComparer.Default.Equals(
                           semanticModel.GetSymbolInfo( a.Argument ).Symbol,
                           aspectReference.ContainingSemantic.Symbol.Parameters[a.Index] ) ));

    public override bool IsValidForTargetSymbol( ISymbol symbol )
        => symbol is IMethodSymbol { MethodKind: not MethodKind.Constructor, AssociatedSymbol: null, IsAsync: false } methodSymbol
           && !IteratorHelper.IsIteratorMethod( methodSymbol );

    public override bool IsValidForContainingSymbol( ISymbol symbol ) => true;
}