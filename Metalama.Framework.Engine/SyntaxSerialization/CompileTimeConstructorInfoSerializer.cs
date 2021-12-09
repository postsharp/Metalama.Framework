// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.ReflectionMocks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class CompileTimeConstructorInfoSerializer : MetalamaMethodBaseSerializer<CompileTimeConstructorInfo, ConstructorInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeConstructorInfo obj, SyntaxSerializationContext serializationContext )
            => SyntaxFactory.ParenthesizedExpression(
                SyntaxFactory.CastExpression(
                    serializationContext.GetTypeSyntax( typeof(ConstructorInfo) ),
                    SerializeMethodBase( obj, serializationContext ) ) );

        public CompileTimeConstructorInfoSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}