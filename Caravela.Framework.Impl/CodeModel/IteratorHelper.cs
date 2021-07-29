// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static partial class IteratorHelper
    {
        public static IteratorInfo GetIteratorInfoImpl( this IMethodSymbol methodSymbol, IMethod? method = null )
        {
            var typeDefinition = methodSymbol.ReturnType.OriginalDefinition;

            var iteratorKind =
                typeDefinition.SpecialType switch
                {
                    SpecialType.System_Collections_IEnumerable => IteratorKind.UntypedIEnumerable,
                    SpecialType.System_Collections_IEnumerator => IteratorKind.UntypedIEnumerator,
                    SpecialType.System_Collections_Generic_IEnumerable_T => IteratorKind.IEnumerable,
                    SpecialType.System_Collections_Generic_IEnumerator_T => IteratorKind.IEnumerator,
                    _ => typeDefinition.Name switch
                    {
                        "IAsyncEnumerable" when methodSymbol.IsAsync && typeDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                            => IteratorKind.IAsyncEnumerable,
                        "IAsyncEnumerator" when methodSymbol.IsAsync && typeDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                            => IteratorKind.IAsyncEnumerator,
                        _ => IteratorKind.None
                    }
                };

            if ( iteratorKind == IteratorKind.None )
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

            if ( methodSymbol.DeclaringSyntaxReferences.Any(
                r => r.GetSyntax() is MethodDeclarationSyntax { Body: { } body } &&
                     FindYieldVisitor.Instance.VisitBlock( body ) ) )
            {
                return new IteratorInfo(
                    iteratorKind,
                    method,
                    ResolveItemType );
            }
            else
            {
                return default;
            }
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

            return symbol.GetIteratorInfoImpl();
        }

        private static IType ResolveItemType( object arg )
        {
            var method = (IMethod) arg;

            if ( method.ReturnType is INamedType { IsGeneric: true } namedType )
            {
                return namedType.GenericArguments[0];
            }
            else
            {
                return method.Compilation.TypeFactory.GetSpecialType( Code.SpecialType.Object );
            }
        }
    }
}