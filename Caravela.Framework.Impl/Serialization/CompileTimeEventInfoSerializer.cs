// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class CompileTimeEventInfoSerializer : ObjectSerializer<CompileTimeEventInfo>
    {
        public override ExpressionSyntax Serialize( CompileTimeEventInfo obj, SyntaxSerializationContext serializationContext )
        {
            var @event = obj.Target.GetTarget( serializationContext.CompilationModel ).AssertNotNull();

            var eventName = @event.Name;
            var typeCreation = this.Service.Serialize( CompileTimeType.Create( @event.DeclaringType ), serializationContext );

            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        typeCreation,
                        IdentifierName( "GetEvent" ) ) )
                .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( eventName ) ) ) )
                .NormalizeWhitespace();
        }

        public CompileTimeEventInfoSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ImmutableArray<Type> AdditionalSupportedTypes => ImmutableArray.Create( typeof(MemberInfo) );
    }
}