// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.CodeModel;

internal static partial class IteratorHelper
{
    public static IteratorInfo GetIteratorInfoImpl( this IMethod method )
    {
        var enumerableKind = GetEnumerableKind( method.ReturnType );

        if ( enumerableKind == EnumerableKind.None )
        {
            return default;
        }
        else
        {
            return new IteratorInfo( method.IsIteratorMethod(), enumerableKind, method.ReturnType );
        }
    }

    public static bool IsIteratorMethod( MethodDeclarationSyntax method )
        => method switch
        {
            { Body: { } body } => FindYieldVisitor.Instance.VisitBlock( body ),
            _ => false
        };

    // TODO: Check why this is not used.
    // Resharper disable once UnusedMember.Global
    public static bool IsIteratorMethod( AccessorDeclarationSyntax accessor )
        => accessor switch
        {
            { Body: { } body } => FindYieldVisitor.Instance.VisitBlock( body ),
            _ => false
        };

    public static bool? IsIteratorMethod( this IMethod method ) => ((IMethodImpl) method).IsIteratorMethod;

    public static bool IsIteratorMethod( IMethodSymbol method )
    {
        if ( method.IsAsync &&
             (method.ReturnType.OriginalDefinition.GetFullName() == "System.Collections.Generic.IAsyncEnumerable"
              || method.ReturnType.OriginalDefinition.GetFullName() == "System.Collections.Generic.IAsyncEnumerator") )
        {
            // Async method that returns IAsyncEnumerable/tor is always an iterator even when no yield is present.
            return true;
        }

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

    private static EnumerableKind GetEnumerableKind( IType returnType ) => GetEnumerableKind( returnType.GetSymbol() );

    private static EnumerableKind GetEnumerableKind( ITypeSymbol returnType )
        => returnType.OriginalDefinition.SpecialType switch
        {
            SpecialType.System_Collections_IEnumerable => EnumerableKind.UntypedIEnumerable,
            SpecialType.System_Collections_IEnumerator => EnumerableKind.UntypedIEnumerator,
            SpecialType.System_Collections_Generic_IEnumerable_T => EnumerableKind.IEnumerable,
            SpecialType.System_Collections_Generic_IEnumerator_T => EnumerableKind.IEnumerator,

            _ => returnType.Name switch
            {
                "IAsyncEnumerable" when returnType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                    => EnumerableKind.IAsyncEnumerable,
                "IAsyncEnumerator" when returnType.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
                    => EnumerableKind.IAsyncEnumerator,
                _ => EnumerableKind.None
            }
        };
}