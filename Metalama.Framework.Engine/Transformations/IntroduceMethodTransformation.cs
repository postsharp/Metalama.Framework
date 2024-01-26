﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal sealed class IntroduceMethodTransformation : IntroduceMemberTransformation<MethodBuilder>
{
    public IntroduceMethodTransformation( Advice advice, MethodBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
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

            return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
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
                        SyntaxFactoryEx.TokenWithSpace( methodBuilder.OperatorKind.ToOperatorKeyword() ),
                        SyntaxFactoryEx.TokenWithSpace( SyntaxKind.OperatorKeyword ),
                        context.SyntaxGenerator.Type( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ).WithTrailingTriviaIfNecessary( ElasticSpace, context.SyntaxGenerationContext.NormalizeWhitespace ),
                        context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                        null,
                        ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ) ),
                        Token( SyntaxKind.SemicolonToken ) );

                return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
            }
            else
            {
                Invariant.Assert( methodBuilder.Parameters.Count is 1 or 2 );

                var syntax =
                    OperatorDeclaration(
                        methodBuilder.GetAttributeLists( context ),
                        TokenList(
                            SyntaxFactoryEx.TokenWithSpace( SyntaxKind.PublicKeyword ),
                            SyntaxFactoryEx.TokenWithSpace( SyntaxKind.StaticKeyword ) ),
                        context.SyntaxGenerator.Type( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ).WithTrailingTriviaIfNecessary( ElasticSpace, context.SyntaxGenerationContext.NormalizeWhitespace ),
                        SyntaxFactoryEx.TokenWithSpace( SyntaxKind.OperatorKeyword ),
                        SyntaxFactoryEx.TokenWithSpace( methodBuilder.OperatorKind.ToOperatorKeyword() ),
                        context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                        null,
                        ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ) ),
                        Token( SyntaxKind.SemicolonToken ) );

                return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
            }
        }
        else
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            // Async iterator can have empty body and still be in iterator, returning anything is invalid.
            var block = SyntaxFactoryEx.FormattedBlock(
                !methodBuilder.ReturnParameter.Type.Is( typeof(void) ) 
                    ? methodBuilder.GetIteratorInfo().IsIteratorMethod == true
                        ?
                        [
                            SyntaxFactoryEx.FormattedBlock(
                                YieldStatement(
                                    SyntaxKind.YieldBreakStatement,
                                    List<AttributeListSyntax>(),
                                    SyntaxFactoryEx.TokenWithSpace( SyntaxKind.YieldKeyword ),
                                    SyntaxFactoryEx.TokenWithSpace( SyntaxKind.BreakKeyword ),
                                    null,
                                    Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList() ) ) )
                        ]
                        : 
                        [
                            ReturnStatement(
                                SyntaxFactoryEx.TokenWithSpace( SyntaxKind.ReturnKeyword ),
                                DefaultExpression( syntaxGenerator.Type( methodBuilder.ReturnParameter.Type.GetSymbol() ) ),
                                Token( SyntaxKind.SemicolonToken ) )
                        ]
                    : [] );

            var method =
                MethodDeclaration(
                    methodBuilder.GetAttributeLists( context ),
                    methodBuilder.GetSyntaxModifierList(),
                    context.SyntaxGenerator.ReturnType( methodBuilder ).WithTrailingTriviaIfNecessary( ElasticSpace, context.SyntaxGenerationContext.NormalizeWhitespace ),
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

            return new[] { new InjectedMember( this, method, this.ParentAdvice.AspectLayerId, InjectedMemberSemantic.Introduction, methodBuilder ) };
        }
    }
}