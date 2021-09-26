// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class AsyncHelper
    {
        public static AsyncInfo GetAsyncInfoImpl( this IMethod method )
        {
            var isAwaitable = TryGetAsyncInfo( method.ReturnType, out var resultType, out var hasMethodBuilder );

            return new AsyncInfo( method.IsAsync, isAwaitable, resultType ?? method.ReturnType, hasMethodBuilder );
        }

        public static AsyncInfo GetAsyncInfoImpl( this IType type )
        {
            var isAwaitable = TryGetAsyncInfo( type, out var resultType, out var hasMethodBuilder );

            return new AsyncInfo( false, isAwaitable, resultType ?? type, hasMethodBuilder );
        }

        // Caches the result type of an awaitable for a type, or null if the type is not awaitable.
        private static readonly ConditionalWeakTable<INamedTypeSymbol, AsyncInfoSymbol?> _cache = new();

        /// <summary>
        /// Gets the type of the result of an awaitable.
        /// </summary>
        /// <param name="awaitableType"></param>
        /// <returns></returns>
        private static bool TryGetAsyncInfo( IType awaitableType, out IType? awaitableResultType, out bool hasMethodBuilder )
        {
            var returnType = awaitableType.GetSymbol();

            if ( !TryGetAsyncInfo( returnType, out var resultTypeSymbol, out hasMethodBuilder ) )
            {
                awaitableResultType = null;

                return false;
            }
            else
            {
                awaitableResultType = ((CompilationModel) awaitableType.Compilation).Factory.GetIType( resultTypeSymbol );

                return true;
            }
        }

        private static bool TryGetAsyncInfo( ITypeSymbol returnType, [NotNullWhen( true )] out ITypeSymbol? resultType, out bool hasMethodBuilder )
        {
            if ( returnType is not INamedTypeSymbol namedType )
            {
                resultType = null;
                hasMethodBuilder = false;

                return false;
            }

            // We're caching because we're always requesting the same few types.
            // ReSharper disable once InconsistentlySynchronizedField
            var cached = _cache.GetValue( namedType, GetAwaitableResultTypeCore );

            if ( cached != null )
            {
                resultType = cached.ResultType;
                hasMethodBuilder = cached.HasMethodBuilder;

                return true;
            }
            else
            {
                resultType = null;
                hasMethodBuilder = false;

                return false;
            }
        }

        private static AsyncInfoSymbol? GetAwaitableResultTypeCore( INamedTypeSymbol returnType )
        {
            var getAwaiterMethod = returnType.GetMembers( "GetAwaiter" ).OfType<IMethodSymbol>().FirstOrDefault( p => p.Parameters.Length == 0 );

            if ( getAwaiterMethod == null )
            {
                return null;
            }

            // The Task type does not have any AsyncMethodBuilder attribute so they need to be marked manually.
            var isTask = returnType.OriginalDefinition.Name == nameof(Task)
                         && returnType.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks";

            // Other types could have an AsyncMethodBuilderAttribute. 
            var hasBuilder = isTask ||
                             returnType.OriginalDefinition.GetAttributes().Any( a => a.AttributeClass?.Name == nameof(AsyncMethodBuilderAttribute) );

            var awaiterType = getAwaiterMethod.ReturnType;
            var getResultMethod = awaiterType.GetMembers( "GetResult" ).OfType<IMethodSymbol>().FirstOrDefault( p => p.Parameters.Length == 0 );

            if ( getResultMethod == null )
            {
                return null;
            }

            return new AsyncInfoSymbol( getResultMethod.ReturnType, hasBuilder );
        }

        /// <summary>
        /// Gets the return type of intermediate methods introduced by the linker or by transformations. The difficulty is that void async methods
        /// must be transformed into async methods returning a ValueType.
        /// </summary>
        public static TypeSyntax GetIntermediateMethodReturnType( IMethodSymbol method, TypeSyntax? returnTypeSyntax, SyntaxGenerationContext generationContext )
            => method.IsAsync && method.ReturnsVoid
                ? generationContext.SyntaxGenerator.Type( generationContext.ReflectionMapper.GetTypeSymbol( typeof(ValueTask) ) )
                : returnTypeSyntax ?? generationContext.SyntaxGenerator.Type( method.ReturnType );

        private record AsyncInfoSymbol( ITypeSymbol ResultType, bool HasMethodBuilder );
    }
}