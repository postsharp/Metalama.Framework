// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.CodeModel
{
    internal static class AsyncHelper
    {
        public static AsyncInfo GetAsyncInfoImpl( this IMethod method )
        {
            var isAwaitable = TryGetAwaitableResultType( method.ReturnType, out var resultType );

            return new AsyncInfo( method.IsAsync, isAwaitable, resultType ?? method.ReturnType );
        }

        // Caches the result type of an awaitable for a type, or null if the type is not awaitable.
        private static readonly ConditionalWeakTable<INamedTypeSymbol, ITypeSymbol?> _cache = new();

        /// <summary>
        /// Gets the type of the result of an awaitable.
        /// </summary>
        /// <param name="awaitableType"></param>
        /// <returns></returns>
        public static bool TryGetAwaitableResultType( IType awaitableType, out IType? awaitableResultType )
        {
            var returnType = awaitableType.GetSymbol();

            if ( !TryGetAwaitableResultTypeSymbol( returnType, out var resultTypeSymbol ) )
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

        private static bool TryGetAwaitableResultTypeSymbol( ITypeSymbol returnType, [NotNullWhen( true )] out ITypeSymbol? returnTypeSymbol )
        {
            if ( returnType is not INamedTypeSymbol namedType )
            {
                returnTypeSymbol = null;

                return false;
            }

            // We're caching because we're always requesting the same few types.
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !_cache.TryGetValue( namedType, out returnTypeSymbol ) )
            {
                lock ( _cache )
                {
                    if ( !_cache.TryGetValue( namedType, out returnTypeSymbol ) )
                    {
                        returnTypeSymbol = GetAwaitableResultTypeCore( namedType );

                        _cache.Add( namedType, returnTypeSymbol );
                    }
                }
            }

            return returnTypeSymbol != null;
        }

        private static ITypeSymbol? GetAwaitableResultTypeCore( INamedTypeSymbol returnType )
        {
            var getAwaiterMethod = returnType.GetMembers( "GetAwaiter" ).OfType<IMethodSymbol>().FirstOrDefault( p => p.Parameters.Length == 0 );

            if ( getAwaiterMethod == null )
            {
                return null;
            }

            var awaiterType = getAwaiterMethod.ReturnType;
            var getResultMethod = awaiterType.GetMembers( "GetResult" ).OfType<IMethodSymbol>().FirstOrDefault( p => p.Parameters.Length == 0 );

            if ( getResultMethod == null )
            {
                return null;
            }

            return getResultMethod.ReturnType;
        }

        /// <summary>
        /// Gets the return type of intermediate methods introduced by the linker or by transformations. The difficulty is that void async methods
        /// must be transformed into async methods returning a ValueType.
        /// </summary>
        public static TypeSyntax GetIntermediateMethodReturnType( Compilation compilation, IMethodSymbol method, TypeSyntax? returnTypeSyntax )
            => method.IsAsync && method.ReturnsVoid
                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( ReflectionMapper.GetInstance( compilation ).GetTypeSymbol( typeof(ValueTask) ) )
                : returnTypeSyntax ?? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( method.ReturnType );

        public static TypeSyntax GetIntermediateMethodReturnType( IMethod method )
            => method.IsAsync && TypeExtensions.Equals( method.ReturnType, SpecialType.Void )
                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression(
                    ReflectionMapper.GetInstance( method.GetCompilationModel().RoslynCompilation ).GetTypeSymbol( typeof(ValueTask) ) )
                : SyntaxHelpers.CreateSyntaxForReturnType( method );
    }
}