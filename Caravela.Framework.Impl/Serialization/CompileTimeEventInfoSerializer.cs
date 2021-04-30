// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CompileTimeEventInfoSerializer : TypedObjectSerializer<CompileTimeEventInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeEventInfo o, ISyntaxFactory syntaxFactory )
        {
            var eventName = o.Symbol.Name;
            var typeCreation = this.Service.Serialize( CompileTimeType.Create( o.ContainingType ), syntaxFactory );

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        typeCreation,
                        IdentifierName( "GetEvent" ) ) )
                .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( eventName ) ) ) )
                .NormalizeWhitespace();
        }

        public CompileTimeEventInfoSerializer( SyntaxSerializationService service ) : base( service ) { }
    }
}