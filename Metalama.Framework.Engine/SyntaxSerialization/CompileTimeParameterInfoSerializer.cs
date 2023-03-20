// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal sealed class CompileTimeParameterInfoSerializer : ObjectSerializer<CompileTimeParameterInfo, ParameterInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeParameterInfo obj, SyntaxSerializationContext serializationContext )
        {
            var parameter = obj.Target.GetTarget( serializationContext.CompilationModel ).AssertNotNull();
            var declaringMember = parameter.DeclaringMember;
            var method = declaringMember as IMethodBase;
            var ordinal = parameter.Index;

            if ( method == null )
            {
                if ( declaringMember is IHasAccessors property )
                {
                    method = (property.GetAccessor( MethodKind.PropertyGet ) ?? property.GetAccessor( MethodKind.PropertySet ))!;
                }
                else
                {
                    throw new AssertionFailedException( $"Unexpected declaration type for '{declaringMember}'." );
                }
            }

            var retrieveMethodBase = CompileTimeMethodInfoSerializer.SerializeMethodBase(
                method,
                serializationContext );

            return ElementAccessExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            retrieveMethodBase,
                            IdentifierName( "GetParameters" ) ) ) )
                .WithArgumentList(
                    BracketedArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( ordinal ) ) ) ) ) )
                .NormalizeWhitespace();
        }

        public CompileTimeParameterInfoSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}