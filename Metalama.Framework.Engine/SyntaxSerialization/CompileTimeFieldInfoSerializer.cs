// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.SyntaxSerialization;

internal class CompileTimeFieldInfoSerializer : ObjectSerializer<CompileTimeFieldInfo, FieldInfo>
{
    public CompileTimeFieldInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

    public override ExpressionSyntax Serialize( CompileTimeFieldInfo obj, SyntaxSerializationContext serializationContext )
    {
        var field = obj.Target.GetTarget( serializationContext.CompilationModel ).AssertNotNull();

        return SerializeField( field, serializationContext );
    }

    public static ExpressionSyntax SerializeField( IField field, SyntaxSerializationContext serializationContext )
    {
        var typeCreation = TypeSerializationHelper.SerializeTypeSymbolRecursive( field.DeclaringType.GetSymbol(), serializationContext );
        var allBindingFlags = SyntaxUtility.CreateBindingFlags( field, serializationContext );

        var fieldInfo = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeCreation,
                    IdentifierName( "GetField" ) ) )
            .AddArgumentListArguments(
                Argument(
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal( field.Name ) ) ),
                Argument( allBindingFlags ) )
            .NormalizeWhitespace();

        return fieldInfo;
    }
}