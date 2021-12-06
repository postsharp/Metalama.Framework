// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CompileTimeMethodInfoSerializer : CaravelaMethodBaseSerializer<CompileTimeMethodInfo, MethodInfo>
    {
        public CompileTimeMethodInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( CompileTimeMethodInfo obj, SyntaxSerializationContext serializationContext )
            => ParenthesizedExpression(
                CastExpression(
                    serializationContext.GetTypeSyntax( typeof(MethodInfo) ),
                    SerializeMethodBase( obj, serializationContext ) ) );
    }
}