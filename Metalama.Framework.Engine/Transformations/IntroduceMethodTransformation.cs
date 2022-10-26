// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal class IntroduceMethodTransformation : IntroduceMemberTransformation<MethodBuilder>
{
    public IntroduceMethodTransformation( Advice advice, MethodBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMember> GetIntroducedMembers( MemberInjectionContext context )
    {
        var methodBuilder = this.IntroducedDeclaration;

        if ( methodBuilder.DeclarationKind == DeclarationKind.Finalizer )
        {
            var syntax =
                DestructorDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList(),
                    ((TypeDeclarationSyntax) methodBuilder.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                    ParameterList(),
                    Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                    null );

            return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
        }
        else if ( methodBuilder.DeclarationKind == DeclarationKind.Operator )
        {
            if ( methodBuilder.OperatorKind.GetCategory() == OperatorCategory.Conversion )
            {
                Invariant.Assert( methodBuilder.Parameters.Count == 1 );

                var syntax =
                    ConversionOperatorDeclaration(
                        methodBuilder.GetAttributeLists( context )
                            .AddRange( methodBuilder.ReturnParameter.GetAttributeLists( context ) ),
                        TokenList(
                            Token( TriviaList(), SyntaxKind.PublicKeyword, TriviaList(ElasticSpace) ),
                            Token( TriviaList(), SyntaxKind.StaticKeyword, TriviaList( ElasticSpace ) ) ),
                        methodBuilder.OperatorKind.ToOperatorKeyword().WithTrailingTrivia( Space ),
                        Token( SyntaxKind.OperatorKeyword ).WithTrailingTrivia( Space ),
                        context.SyntaxGenerator.Type( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ).WithTrailingTrivia( Space ),
                        context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                        null,
                        ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ) ),
                        Token( SyntaxKind.SemicolonToken ) );

                return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
            }
            else
            {
                Invariant.Assert( methodBuilder.Parameters.Count is 1 or 2 );

                var syntax =
                    OperatorDeclaration(
                        methodBuilder.GetAttributeLists( context )
                            .AddRange( methodBuilder.ReturnParameter.GetAttributeLists( context ) ),
                        TokenList(
                            Token( TriviaList(), SyntaxKind.PublicKeyword, TriviaList( ElasticSpace ) ),
                            Token( TriviaList(), SyntaxKind.StaticKeyword, TriviaList( ElasticSpace ) ) ),
                        context.SyntaxGenerator.Type( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ).WithTrailingTrivia( Space ),
                        Token( SyntaxKind.OperatorKeyword ).WithTrailingTrivia( Space ),
                        methodBuilder.OperatorKind.ToOperatorKeyword().WithTrailingTrivia( Space ),
                        context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                        null,
                        ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ) ),
                        Token( SyntaxKind.SemicolonToken ) );

                return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
            }
        }
        else
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            var block = SyntaxFactoryEx.FormattedBlock(
                !methodBuilder.ReturnParameter.Type.Is( typeof( void ) )
                    ? new StatementSyntax[]
                    {
                            ReturnStatement(
                                Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                                DefaultExpression( syntaxGenerator.Type( methodBuilder.ReturnParameter.Type.GetSymbol() ) ),
                                Token( SyntaxKind.SemicolonToken ) )
                    }
                    : Array.Empty<StatementSyntax>() );

            var method =
                MethodDeclaration(
                    methodBuilder.GetAttributeLists( context )
                        .AddRange( methodBuilder.ReturnParameter.GetAttributeLists( context ) ),
                    methodBuilder.GetSyntaxModifierList(),
                    context.SyntaxGenerator.ReturnType( methodBuilder ).WithTrailingTrivia( Space ),
                    methodBuilder.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier(
                            (NameSyntax) syntaxGenerator.Type( methodBuilder.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                        : null,
                    methodBuilder.GetCleanName(),
                    context.SyntaxGenerator.TypeParameterList( methodBuilder, context.Compilation ),
                    context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                    context.SyntaxGenerator.ConstraintClauses( methodBuilder ),
                    block,
                    null );

            return new[] { new InjectedMember( this, method, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
        }
    }
}