// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

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
                SyntaxFactory.DestructorDeclaration(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    SyntaxFactory.TokenList(),
                    ((TypeDeclarationSyntax) methodBuilder.DeclaringType.GetPrimaryDeclarationSyntax().AssertNotNull()).Identifier,
                    SyntaxFactory.ParameterList(),
                    SyntaxFactory.Block().WithGeneratedCodeAnnotation( this.ParentAdvice.Aspect.AspectClass.GeneratedCodeAnnotation ),
                    null );

            return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
        }
        else if ( methodBuilder.DeclarationKind == DeclarationKind.Operator )
        {
            if ( methodBuilder.OperatorKind.GetCategory() == OperatorCategory.Conversion )
            {
                Invariant.Assert( methodBuilder.Parameters.Count == 1 );

                var syntax =
                    SyntaxFactory.ConversionOperatorDeclaration(
                        methodBuilder.GetAttributeLists( context )
                            .AddRange( methodBuilder.ReturnParameter.GetAttributeLists( context ) ),
                        SyntaxFactory.TokenList( SyntaxFactory.Token( SyntaxKind.PublicKeyword ), SyntaxFactory.Token( SyntaxKind.StaticKeyword ) ),
                        methodBuilder.OperatorKind.ToOperatorKeyword(),
                        context.SyntaxGenerator.Type( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ),
                        context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                        null,
                        SyntaxFactory.ArrowExpressionClause(
                            context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ) ) );

                return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
            }
            else
            {
                Invariant.Assert( methodBuilder.Parameters.Count is 1 or 2 );

                var syntax =
                    SyntaxFactory.OperatorDeclaration(
                        methodBuilder.GetAttributeLists( context )
                            .AddRange( methodBuilder.ReturnParameter.GetAttributeLists( context ) ),
                        SyntaxFactory.TokenList( SyntaxFactory.Token( SyntaxKind.PublicKeyword ), SyntaxFactory.Token( SyntaxKind.StaticKeyword ) ),
                        context.SyntaxGenerator.Type( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ),
                        methodBuilder.OperatorKind.ToOperatorKeyword(),
                        context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                        null,
                        SyntaxFactory.ArrowExpressionClause(
                            context.SyntaxGenerator.DefaultExpression( methodBuilder.ReturnType.GetSymbol().AssertNotNull() ) ) );

                return new[] { new InjectedMember( this, syntax, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
            }
        }
        else
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            var method =
                SyntaxFactory.MethodDeclaration(
                    methodBuilder.GetAttributeLists( context )
                        .AddRange( methodBuilder.ReturnParameter.GetAttributeLists( context ) ),
                    methodBuilder.GetSyntaxModifierList(),
                    context.SyntaxGenerator.ReturnType( methodBuilder ),
                    methodBuilder.ExplicitInterfaceImplementations.Count > 0
                        ? SyntaxFactory.ExplicitInterfaceSpecifier(
                            (NameSyntax) syntaxGenerator.Type( methodBuilder.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                        : null,
                    this.IntroducedDeclaration.GetCleanName(),
                    context.SyntaxGenerator.TypeParameterList( methodBuilder, context.Compilation ),
                    context.SyntaxGenerator.ParameterList( methodBuilder, context.Compilation ),
                    context.SyntaxGenerator.ConstraintClauses( methodBuilder ),
                    SyntaxFactory.Block(
                        SyntaxFactory.List(
                            !methodBuilder.ReturnParameter.Type.Is( typeof(void) )
                                ? new[]
                                {
                                    SyntaxFactory.ReturnStatement(
                                        SyntaxFactory.Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( SyntaxFactory.Whitespace( " " ) ),
                                        SyntaxFactory.DefaultExpression( syntaxGenerator.Type( methodBuilder.ReturnParameter.Type.GetSymbol() ) ),
                                        SyntaxFactory.Token( SyntaxKind.SemicolonToken ) )
                                }
                                : Array.Empty<StatementSyntax>() ) ),
                    null );

            return new[] { new InjectedMember( this, method, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, methodBuilder ) };
        }
    }
}