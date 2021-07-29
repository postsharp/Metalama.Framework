// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Transformations
{
    internal static class AsyncHelper
    {
        private static readonly ConditionalWeakTable<INamedTypeSymbol, ITypeSymbol> _cache = new();

        public static IType GetTaskResultType( IType taskType )
        {
            switch ( ((INamedType) taskType).OriginalDeclaration.SpecialType )
            {
                case SpecialType.Void:
                case SpecialType.IAsyncEnumerable:
                case SpecialType.IAsyncEnumerator:
                    // In these cases there is no task but the "unwrapped" return type is the return type itself. 
                    return taskType;
            }

            INamedTypeSymbol returnType = (INamedTypeSymbol) taskType.GetSymbol();

            // We're caching because we're always requesting the same few types.
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !_cache.TryGetValue( returnType, out var returnTypeSymbol ) )
            {
                lock ( _cache )
                {
                    if ( !_cache.TryGetValue( returnType, out returnTypeSymbol ) )
                    {
                        var getAwaiterMethod = returnType.GetMembers( "GetAwaiter" ).OfType<IMethodSymbol>().Single( p => p.Parameters.Length == 0 );
                        var awaiterType = getAwaiterMethod.ReturnType;
                        var getResultMethod = awaiterType.GetMembers( "GetResult" ).OfType<IMethodSymbol>().Single( p => p.Parameters.Length == 0 );

                        returnTypeSymbol = getResultMethod.ReturnType;

                        _cache.Add( returnType, returnTypeSymbol );
                    }
                }
            }

            return ((CompilationModel) taskType.Compilation).Factory.GetIType( returnTypeSymbol );
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
            => method.IsAsync && method.ReturnType.Is( SpecialType.Void )
                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression(
                    ReflectionMapper.GetInstance( method.GetCompilationModel().RoslynCompilation ).GetTypeSymbol( typeof(ValueTask) ) )
                : SyntaxHelpers.CreateSyntaxForReturnType( method );
    }
}