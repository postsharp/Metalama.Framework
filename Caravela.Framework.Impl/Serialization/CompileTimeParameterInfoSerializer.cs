// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CompileTimeParameterInfoSerializer : ObjectSerializer<CompileTimeParameterInfo, ParameterInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeParameterInfo obj, ICompilationElementFactory syntaxFactory )
        {
            var parameter = obj.Target.Resolve( syntaxFactory.CompilationModel );
            var declaringMember = parameter.DeclaringMember;
            var method = declaringMember as IMethodBase;
            var ordinal = parameter.Index;

            if ( method == null && declaringMember is IProperty property )
            {
                method = (property.Getter ?? property.Setter)!;
            }

            var retrieveMethodBase = this.Service.CompileTimeMethodInfoSerializer.SerializeMethodBase(
                method!.GetSymbol(),
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