// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Pseudo;
using Metalama.Framework.Engine.Templating.MetaModel;
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
                return new IteratorInfo( false, iteratorKind, method );
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

        public static bool IsIterator( MethodDeclarationSyntax method )
            => method switch
            {
                { Body: { } body } => FindYieldVisitor.Instance.VisitBlock( body ),
                _ => false
            };

        public static bool IsIterator( AccessorDeclarationSyntax accessor )
            => accessor switch
            {
                { Body: { } body } => FindYieldVisitor.Instance.VisitBlock( body ),
                _ => false
            };

        // We use the Impl suffix to resolve an ambiguity with the public API.
        public static IteratorInfo GetIteratorInfoImpl( this IMethod method )
        {
            // TODO: Maybe this should be a member of IMethodInternal instead of a switch in extension method.
            switch ( method )
            {
                case Method sourceMethod:
                    return sourceMethod.GetSymbol().AssertNotNull().GetIteratorInfoImpl( method );
                
                case BuiltMethod builtMethod:
                    return builtMethod.MethodBuilder.GetIteratorInfoImpl();
                
                case BuiltAccessor builtAccessor:
                    return builtAccessor.AccessorBuilder.GetIteratorInfoImpl();
                
                case AdvisedMethod advisedMethod:
                    return advisedMethod.Underlying.GetIteratorInfoImpl();
                
                case MethodBuilder methodBuilder:
                    return new IteratorInfo( methodBuilder.IsIterator, methodBuilder.EnumerableKind, methodBuilder );
                
                case AccessorBuilder accessorBuilder:
                    return new IteratorInfo( accessorBuilder.IsIterator, accessorBuilder.EnumerableKind, accessorBuilder );
                
                case IPseudoDeclaration:
                    // Pseudo methods are never iterators.
                    return new IteratorInfo( false, EnumerableKind.None, method );
                
                default:
                    throw new AssertionFailedException( $"Unexpected type: {method.GetType().Name}" );
            }
        }
    }
}