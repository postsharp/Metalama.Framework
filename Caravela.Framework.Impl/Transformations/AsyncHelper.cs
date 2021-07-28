// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Transformations
{
    internal static class AsyncHelper
    {
        public static IType GetTaskResultType( IType taskType )
        {
            if ( taskType.Is( SpecialType.Void ) )
            {
                return taskType;
            }
            
            INamedTypeSymbol returnType = (INamedTypeSymbol) taskType.GetSymbol();
            
            var getAwaiterMethod = returnType.GetMembers( "GetAwaiter" ).OfType<IMethodSymbol>().Single( p => p.Parameters.Length == 0 );
            var awaiterType = getAwaiterMethod.ReturnType;
            var getResultMethod = awaiterType.GetMembers( "GetResult" ).OfType<IMethodSymbol>().Single( p => p.Parameters.Length == 0 );

            return ((CompilationModel) taskType.Compilation).Factory.GetIType( getResultMethod.ReturnType );
        }
        
        /// <summary>
        /// Gets the return type of intermediate methods introduced by the linker or by transformations. The difficulty is that void async methods
        /// must be transformed into async methods returning a ValueType.
        /// </summary>
        public static TypeSyntax GetIntermediateMethodReturnType( Compilation compilation, IMethodSymbol method, TypeSyntax? returnTypeSyntax )
            => method.IsAsync && method.ReturnsVoid
                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression(
                    ReflectionMapper.GetInstance( compilation ).GetTypeSymbol( typeof(ValueTask) ) )
                : returnTypeSyntax ?? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( method.ReturnType );
        
        public static TypeSyntax GetIntermediateMethodReturnType( IMethod method )
            => method.IsAsync && method.ReturnType.Is( SpecialType.Void )
                ? LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression(
                    ReflectionMapper.GetInstance( method.GetCompilationModel().RoslynCompilation ).GetTypeSymbol( typeof(ValueTask) ) )
                : SyntaxHelpers.CreateSyntaxForEventType( method );
    }
}