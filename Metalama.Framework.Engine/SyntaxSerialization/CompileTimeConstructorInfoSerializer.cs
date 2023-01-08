// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System.Reflection;

namespace Metalama.Framework.Engine.SyntaxSerialization;

internal sealed class CompileTimeConstructorInfoSerializer : MetalamaMethodBaseSerializer<CompileTimeConstructorInfo, ConstructorInfo>
{
    public override ExpressionSyntax Serialize( CompileTimeConstructorInfo obj, SyntaxSerializationContext serializationContext )
        => SyntaxFactory.ParenthesizedExpression(
                SyntaxFactoryEx.SafeCastExpression(
                    serializationContext.GetTypeSyntax( typeof(ConstructorInfo) ),
                    SerializeMethodBase( obj, serializationContext ) ) )
            .WithAdditionalAnnotations( Simplifier.Annotation );

    public CompileTimeConstructorInfoSerializer( SyntaxSerializationService service ) : base( service ) { }
}