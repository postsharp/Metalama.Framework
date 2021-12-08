// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static partial class IteratorHelper
    {
        public static IteratorInfo GetIteratorInfoImpl( this IMethodSymbol methodSymbol, IMethod? method = null )
        {
            var typeDefinition = methodSymbol.ReturnType.OriginalDefinition;

            var iteratorKind =
                typeDefinition.SpecialType switch
                {
                    SpecialType.System_Collections_IEnumerable => EnumerableKind.UntypedIEnumerable,
                    SpecialType.System_Collections_IEnumerator => EnumerableKind.UntypedIEnumerator,
                    SpecialType.System_Collections_Generic_IEnumerable_T => EnumerableKind.IEnumerable,
                    SpecialType.System_Collections_Generic_IEnumerator_T => EnumerableKind.IEnumerator,
                    _ => typeDefinition.Name switch
                    {
                        "IAsyncEnumerable" when typeDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                            => EnumerableKind.IAsyncEnumerable,
                        "IAsyncEnumerator" when typeDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                            => EnumerableKind.IAsyncEnumerator,
                        _ => EnumerableKind.None
                    }
                };

            if ( iteratorKind == EnumerableKind.None )
            {
                return default;
            }

            if ( methodSymbol.DeclaringSyntaxReferences.IsEmpty )
            {
                // When this is not a source code method, we don't know, but for any applicable case,
                // it should not be different to return that this is not an iterator, because for non-source methods
                // we can assume that this is an implementation detail.
                return default;
            }

            var isIterator = IsIterator( methodSymbol );

            return new IteratorInfo( isIterator, iteratorKind, method );
        }

        public static bool IsIterator( IMethodSymbol method )
        {
            var isIterator = method.DeclaringSyntaxReferences.Any(
                r => r.GetSyntax() switch
                {
                    MethodDeclarationSyntax { Body: { } body } => FindYieldVisitor.Instance.VisitBlock( body ),
                    AccessorDeclarationSyntax { Body: { } body } => FindYieldVisitor.Instance.VisitBlock( body ),
                    _ => false
                } );

            // If we don't have the source code (i.e. if we have a symbol for a compiled assembly), it is safe to return that
            // the method is not a yield-based iterator, because if we are in an external assembly, so this must be considered
            // an implementation detail.

            return isIterator;
        }

        // We use the Impl suffix to resolve an ambiguity with the public API.
        public static IteratorInfo GetIteratorInfoImpl( this IMethod method )
        {
            var symbol = method.GetSymbol();

            if ( symbol == null )
            {
                // We have an introduced method, for which iterators are not supported yet.
                return default;
            }

            return symbol.GetIteratorInfoImpl( method );
        }
    }
}