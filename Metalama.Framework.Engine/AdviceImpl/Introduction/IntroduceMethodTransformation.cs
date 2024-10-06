﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceMethodTransformation : IntroduceMemberTransformation<MethodBuilderData>
{
    public IntroduceMethodTransformation( Advice advice, MethodBuilderData introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var methodBuilder = this.BuilderData.ToRef().GetTarget( context.Compilation );

        var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

        var explicitInterfaceSpecifier = methodBuilder.ExplicitInterfaceImplementations.Count > 0
            ? ExplicitInterfaceSpecifier( (NameSyntax) syntaxGenerator.Type( methodBuilder.ExplicitInterfaceImplementations.Single().DeclaringType ) )
            : null;

        if ( methodBuilder.DeclarationKind == DeclarationKind.Finalizer )
        {
            var syntax = DestructorDeclaration(
                AdviceSyntaxGenerator.GetAttributeLists( methodBuilder, context ),
                TokenList(),
                ((TypeDeclarationSyntax) methodBuilder.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                ParameterList(),
                Block().WithGeneratedCodeAnnotation( this.AspectInstance.AspectClass.GeneratedCodeAnnotation ),
                null );

            return [new InjectedMember( this, syntax, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() )];
        }
        else if ( methodBuilder.DeclarationKind == DeclarationKind.Operator )
        {
            if ( methodBuilder.OperatorKind.GetCategory() == OperatorCategory.Conversion )
            {
                Invariant.Assert( methodBuilder.Parameters.Count == 1 );

                var syntax = ConversionOperatorDeclaration(
                    AdviceSyntaxGenerator.GetAttributeLists( methodBuilder, context ),
                    methodBuilder.GetSyntaxModifierList(),
                    SyntaxFactoryEx.TokenWithTrailingSpace( methodBuilder.OperatorKind.ToOperatorKeyword() ),
                    explicitInterfaceSpecifier,
                    SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.OperatorKeyword ),
                    context.SyntaxGenerator.Type( methodBuilder.ReturnType )
                        .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                    null,
                    ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType ) ),
                    Token( SyntaxKind.SemicolonToken ) );

                return [new InjectedMember( this, syntax, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() )];
            }
            else
            {
                Invariant.Assert( methodBuilder.Parameters.Count is 1 or 2 );

                var syntax = OperatorDeclaration(
                    AdviceSyntaxGenerator.GetAttributeLists( methodBuilder, context ),
                    methodBuilder.GetSyntaxModifierList(),
                    context.SyntaxGenerator.Type( methodBuilder.ReturnType )
                        .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    explicitInterfaceSpecifier,
                    SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.OperatorKeyword ),
                    SyntaxFactoryEx.TokenWithTrailingSpace( methodBuilder.OperatorKind.ToOperatorKeyword() ),
                    context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                    null,
                    ArrowExpressionClause( context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType ) ),
                    Token( SyntaxKind.SemicolonToken ) );

                return [new InjectedMember( this, syntax, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() )];
            }
        }
        else
        {
            // ReSharper disable RedundantLinebreak

            // Async iterator can have empty body and still be in iterator, returning anything is invalid.
            var block = syntaxGenerator.FormattedBlock(
                !methodBuilder.ReturnParameter.Type.Is( typeof(void) )
                    ? methodBuilder.GetIteratorInfo().IsIteratorMethod == true
                        ?
                        [
                            syntaxGenerator.FormattedBlock(
                                YieldStatement(
                                    SyntaxKind.YieldBreakStatement,
                                    List<AttributeListSyntax>(),
                                    SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.YieldKeyword ),
                                    SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.BreakKeyword ),
                                    null,
                                    Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList() ) ) )
                        ]
                        :
                        [
                            ReturnStatement(
                                SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                                DefaultExpression( syntaxGenerator.Type( methodBuilder.ReturnParameter.Type ) ),
                                Token( SyntaxKind.SemicolonToken ) )
                        ]
                    : [] );

            // ReSharper enable RedundantLinebreak

            var method = MethodDeclaration(
                AdviceSyntaxGenerator.GetAttributeLists( methodBuilder, context ),
                methodBuilder.GetSyntaxModifierList(),
                context.SyntaxGenerator.ReturnType( methodBuilder ).WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                explicitInterfaceSpecifier,
                methodBuilder.GetCleanName(),
                context.SyntaxGenerator.TypeParameterList( methodBuilder, context.Compilation ),
                context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                context.SyntaxGenerator.ConstraintClauses( methodBuilder ),
                block,
                null );

            return [new InjectedMember( this, method, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() )];
        }
    }
}