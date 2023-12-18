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

internal sealed class IntroduceMethodTransformation : IntroduceMemberTransformation<MethodBuilder>
{
    public IntroduceMethodTransformation( Advice advice, MethodBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMemberOrNamedType> GetInjectedMembers( MemberInjectionContext context )
    {
        var methodBuilder = this.IntroducedDeclaration;

        if ( methodBuilder.DeclarationKind == DeclarationKind.Finalizer )
        {
            var syntax =
                DestructorDeclaration(
                    methodBuilder.GetAttributeLists( context ),
                    TokenList(),
                    ((TypeDeclarationSyntax) methodBuilder.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                    ParameterList(),
                    Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                    null );

            return new[] { new InjectedMemberOrNamedType( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
        }
        else if ( methodBuilder.DeclarationKind == DeclarationKind.Operator )
        {
            if ( methodBuilder.OperatorKind.GetCategory() == OperatorCategory.Conversion )
            {
                Invariant.Assert( methodBuilder.Parameters.Count == 1 );

                var syntax =
                    ConversionOperatorDeclaration(
                        methodBuilder.GetAttributeLists( context ),
                        TokenList(
                            Token( TriviaList(), SyntaxKind.PublicKeyword, TriviaList( ElasticSpace ) ),
                            Token( TriviaList(), SyntaxKind.StaticKeyword, TriviaList( ElasticSpace ) ) ),
                        methodBuilder.OperatorKind.ToOperatorKeyword().WithTrailingTrivia( Space ),
                        Token( SyntaxKind.OperatorKeyword ).WithTrailingTrivia( Space ),
                        context.SyntaxGenerator.Type( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ).WithTrailingTrivia( Space ),
                        context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                        null,
                        ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ) ),
                        Token( SyntaxKind.SemicolonToken ) );

                return new[] { new InjectedMemberOrNamedType( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
            }
            else
            {
                Invariant.Assert( methodBuilder.Parameters.Count is 1 or 2 );

                var syntax =
                    OperatorDeclaration(
                        methodBuilder.GetAttributeLists( context ),
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

                return new[] { new InjectedMemberOrNamedType( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
            }
        }
        else
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            // Async iterator can have empty body and still be in iterator, returning anything is invalid.
            var block = SyntaxFactoryEx.FormattedBlock(
                !methodBuilder.ReturnParameter.Type.Is( typeof(void) ) 
                    ? methodBuilder.GetIteratorInfo().IsIteratorMethod == true
                        ? new StatementSyntax[]
                        {
                            SyntaxFactoryEx.FormattedBlock(
                                YieldStatement(
                                    SyntaxKind.YieldBreakStatement,
                                    List<AttributeListSyntax>(),
                                    Token( TriviaList(), SyntaxKind.YieldKeyword, TriviaList( ElasticSpace ) ),
                                    Token( TriviaList(), SyntaxKind.BreakKeyword, TriviaList( ElasticSpace ) ),
                                    null,
                                    Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList() ) ) )
                        }
                        : new StatementSyntax[]
                        {
                            ReturnStatement(
                                Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                                DefaultExpression( syntaxGenerator.Type( methodBuilder.ReturnParameter.Type.GetSymbol() ) ),
                                Token( SyntaxKind.SemicolonToken ) )
                        }
                    : Array.Empty<StatementSyntax>() );

            var method =
                MethodDeclaration(
                    methodBuilder.GetAttributeLists( context ),
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

            return new[] { new InjectedMemberOrNamedType( this, method, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
        }
    }
}