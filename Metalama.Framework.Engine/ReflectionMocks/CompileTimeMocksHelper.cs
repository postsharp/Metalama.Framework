// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal static class CompileTimeMocksHelper
    {
        // Coverage: ignore
        public static Exception CreateNotSupportedException( string typeName )
            => new NotSupportedException(
                $"This instance of {typeName} cannot be accessed at compile time because it represents a run-time object. Try using meta.RunTime() to convert this object to its run-time value." );

        public static TypedExpressionSyntax ToTypedExpressionSyntax<T>(
            ICompileTimeReflectionObject<T> compileTimeMemberInfo,
            Func<T, SyntaxSerializationContext, ExpressionSyntax> serialize,
            ISyntaxSerializationContext syntaxSerializationContext )
            where T : class, ICompilationElement
        {
            var serializationContext = (SyntaxSerializationContext) syntaxSerializationContext;

            return ToTypedExpressionSyntax( 
                compileTimeMemberInfo.Target.GetTarget( serializationContext.CompilationModel ),
                compileTimeMemberInfo.ReflectionType,
                serialize, 
                syntaxSerializationContext );
        }

        public static TypedExpressionSyntax ToTypedExpressionSyntax<T>(
            T member,
            Type type,
            Func<T, SyntaxSerializationContext, ExpressionSyntax> serialize,
            ISyntaxSerializationContext syntaxSerializationContext )
        {
            var serializationContext = (SyntaxSerializationContext) syntaxSerializationContext;

            var expression = serialize( member, serializationContext );

            var iType = serializationContext.CompilationModel.Factory.GetTypeByReflectionType( type );

            return new(
                new TypedExpressionSyntaxImpl(
                    expression,
                    iType,
                    serializationContext.SyntaxGenerationContext,
                    true ) );
        }
    }
}