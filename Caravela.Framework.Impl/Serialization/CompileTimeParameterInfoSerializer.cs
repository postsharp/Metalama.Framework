// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CompileTimeParameterInfoSerializer : TypedObjectSerializer<CompileTimeParameterInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeParameterInfo o, ISyntaxFactory syntaxFactory )
        {
            var container = o.DeclaringMember;
            var containerAsMember = container as IMember;
            var method = containerAsMember as IMethodBase;
            var property = containerAsMember as Property;
            var ordinal = o.ParameterSymbol.Ordinal;

            if ( method == null && property != null )
            {
                method = (property.Getter ?? property.Setter)!;
            }

            var retrieveMethodBase = this.Service.CompileTimeMethodInfoSerializer.SerializeMethodBase( 
                (IReflectionMockMember) method!.ToMethodBase(),
                syntaxFactory );

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