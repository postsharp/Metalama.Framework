// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceConstructorTransformation
    : IntroduceMemberTransformation<ConstructorBuilder>, IReplaceMemberTransformation, IInsertStatementTransformation
{
    public IntroduceConstructorTransformation( Advice advice, ConstructorBuilder introducedDeclaration ) : base( advice, introducedDeclaration )
    {
        Invariant.Assert( !introducedDeclaration.IsStatic );
        Invariant.Assert( !introducedDeclaration.IsRecordCopyConstructor() );

        var targetType = introducedDeclaration.DeclaringType;

        if ( introducedDeclaration.IsReplacingImplicit )
        {
            this.ReplacedMember = targetType.Constructors.Single( c => c.IsImplicitInstanceConstructor() ).ToMemberRef<IMember>();
        }
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var constructorBuilder = this.IntroducedDeclaration;

        var statements = Array.Empty<StatementSyntax>();

        var syntaxSerializationContext = new SyntaxSerializationContext(
            context.Compilation,
            context.SyntaxGenerationContext,
            constructorBuilder.DeclaringType.ForCompilation( context.Compilation ) );

        var arguments =
            ArgumentList(
                SeparatedList(
                    constructorBuilder.InitializerArguments.SelectAsArray( a =>
                        Argument(
                            a.ParameterName != null
                            ? NameColon( IdentifierName( a.ParameterName ) )
                            : null,
                            default,
                            a.Expression.ToExpressionSyntax( syntaxSerializationContext ) ) ) ) );

        var initializer =
            constructorBuilder.InitializerKind switch
            {
                ConstructorInitializerKind.None => null,
                ConstructorInitializerKind.Base => 
                    ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        arguments ),
                ConstructorInitializerKind.This =>
                    ConstructorInitializer(
                        SyntaxKind.ThisConstructorInitializer,
                        arguments ),
                var i => throw new AssertionFailedException($"Unsupported initializer kind: {i}"),
            };

        var syntax =
            ConstructorDeclaration(
                constructorBuilder.GetAttributeLists( context ),
                TokenList( Token( TriviaList(), SyntaxKind.PublicKeyword, TriviaList( Space ) ) ),
                Identifier( constructorBuilder.DeclaringType.Name ),
                context.SyntaxGenerator.ParameterList( constructorBuilder, context.Compilation ),
                initializer,
                context.SyntaxGenerationContext.SyntaxGenerator.FormattedBlock( statements )
                    .WithGeneratedCodeAnnotation( this.ParentAdvice.AspectInstance.AspectClass.GeneratedCodeAnnotation ),
                null );

        return new[]
        {
            new InjectedMember(
                this,
                syntax,
                this.ParentAdvice.AspectLayerId,
                InjectedMemberSemantic.Introduction,
                constructorBuilder )
        };
    }

    public MemberRef<IMember> ReplacedMember { get; }

    public override InsertPosition InsertPosition
        => this.ReplacedMember.Target != null
            ? this.ReplacedMember.GetTarget( this.TargetDeclaration.Compilation ).ToInsertPosition()
            : this.IntroducedDeclaration.ToInsertPosition();

    public override TransformationObservability Observability => TransformationObservability.CompileTimeOnly;

    public IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        // See https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#auto-default-struct.
        if ( this.IntroducedDeclaration.DeclaringType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct &&
             context.SyntaxGenerationContext.RequiresStructFieldInitialization )
        {
            return new[]
            {
                // this = default;
                new InsertedStatement(
                    ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                ThisExpression(),
                                LiteralExpression( SyntaxKind.DefaultLiteralExpression ) ) )
                        .WithGeneratedCodeAnnotation( this.ParentAdvice.AspectInstance.AspectClass.GeneratedCodeAnnotation ),
                    this.IntroducedDeclaration,
                    this,
                    InsertedStatementKind.Initializer )
            };
        }

        return Array.Empty<InsertedStatement>();
    }

    IMember IInsertStatementTransformation.TargetMember => this.IntroducedDeclaration;
}