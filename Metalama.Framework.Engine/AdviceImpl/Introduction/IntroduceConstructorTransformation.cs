// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
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
    : IntroduceMemberTransformation<ConstructorBuilderData>, IReplaceMemberTransformation, IInsertStatementTransformation
{
    public IntroduceConstructorTransformation( AdviceInfo advice, ConstructorBuilderData introducedDeclaration ) : base( advice, introducedDeclaration )
    {
        Invariant.Assert( !introducedDeclaration.IsStatic );

        this.ReplacedMember = introducedDeclaration.ReplacedImplicitConstructor;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var constructorBuilder = this.BuilderData.ToRef().GetTarget( context.Compilation );

        Invariant.Assert( !constructorBuilder.IsRecordCopyConstructor() );

        var statements = Array.Empty<StatementSyntax>();

        var syntaxSerializationContext = new SyntaxSerializationContext(
            context.Compilation,
            context.SyntaxGenerationContext,
            constructorBuilder.DeclaringType );

        var arguments =
            ArgumentList(
                SeparatedList(
                    this.BuilderData.InitializerArguments.SelectAsArray(
                        a =>
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
                var i => throw new AssertionFailedException( $"Unsupported initializer kind: {i}" )
            };

        var syntax =
            ConstructorDeclaration(
                AdviceSyntaxGenerator.GetAttributeLists( constructorBuilder, context ),
                constructorBuilder.GetSyntaxModifierList(),
                Identifier( constructorBuilder.DeclaringType.Name ),
                context.SyntaxGenerator.ParameterList( constructorBuilder, context.Compilation ),
                initializer,
                context.SyntaxGenerationContext.SyntaxGenerator.FormattedBlock( statements )
                    .WithGeneratedCodeAnnotation( this.AspectInstance.AspectClass.GeneratedCodeAnnotation ),
                null );

        return
        [
            new InjectedMember(
                this,
                syntax,
                this.AspectLayerId,
                InjectedMemberSemantic.Introduction,
                this.BuilderData.ToRef() )
        ];
    }

    public IRef<IMember>? ReplacedMember { get; }

    public override InsertPosition InsertPosition => this.ReplacedMember?.ToInsertPosition() ?? this.BuilderData.InsertPosition;

    public override TransformationObservability Observability
        => this.ReplacedMember == null
            ? TransformationObservability.Always
            : TransformationObservability.CompileTimeOnly;

    public IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context )
    {
        var constructorBuilder = this.BuilderData.ToRef().GetTarget( context.Compilation );

        // See https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#auto-default-struct.
        if ( constructorBuilder.DeclaringType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct &&
             context.SyntaxGenerationContext.RequiresStructFieldInitialization )
        {
            return
            [
                // this = default;
                new InsertedStatement(
                    ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                ThisExpression(),
                                LiteralExpression( SyntaxKind.DefaultLiteralExpression ) ) )
                        .WithGeneratedCodeAnnotation( this.AspectInstance.AspectClass.GeneratedCodeAnnotation ),
                    constructorBuilder,
                    this,
                    InsertedStatementKind.Initializer )
            ];
        }

        return Array.Empty<InsertedStatement>();
    }

    IFullRef<IMember> IInsertStatementTransformation.TargetMember => this.BuilderData.ToRef();
}