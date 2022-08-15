﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using RefKind = Metalama.Framework.Code.RefKind;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Transformations
{
    internal abstract class OverrideMethodBaseTransformation : OverrideMemberTransformation
    {
        protected new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        protected OverrideMethodBaseTransformation( Advice advice, IMethod targetMethod, IObjectReader tags )
            : base( advice, targetMethod, tags ) { }

        protected BuiltUserExpression CreateProceedExpression( in MemberIntroductionContext context, TemplateKind templateKind )
        {
            return ProceedHelper.CreateProceedDynamicExpression(
                context.SyntaxGenerationContext,
                this.CreateInvocationExpression( context.SyntaxGenerationContext ),
                templateKind,
                this.OverriddenDeclaration );
        }

        protected IntroducedMember[] GetIntroducedMembersImpl( in MemberIntroductionContext context, BlockSyntax newMethodBody, bool isAsyncTemplate )
        {
            TypeSyntax? returnType = null;

            var modifiers = this.OverriddenDeclaration.GetSyntaxModifierList();

            if ( !this.OverriddenDeclaration.IsAsync )
            {
                if ( isAsyncTemplate )
                {
                    // If the template is async but the overridden declaration is not, we have to add an async modifier.
                    modifiers = modifiers.Add( Token( SyntaxKind.AsyncKeyword ) );
                }
            }
            else
            {
                if ( !isAsyncTemplate )
                {
                    // If the template is not async but the overridden declaration is, we have to remove the async modifier.
                    modifiers = TokenList( modifiers.Where( m => !m.IsKind( SyntaxKind.AsyncKeyword ) ) );
                }

                // If the template is async and the target declaration is `async void`, and regardless of the async flag the template, we have to change the type to ValueTask, otherwise
                // it is not awaitable

                if ( this.OverriddenDeclaration.ReturnType.Equals( SpecialType.Void ) )
                {
                    returnType = context.SyntaxGenerator.Type(
                        this.OverriddenDeclaration.GetCompilationModel().Factory.GetSpecialType( SpecialType.ValueTask ).GetSymbol() );
                }
            }

            returnType ??= context.SyntaxGenerator.Type( this.OverriddenDeclaration.ReturnType.GetSymbol() );

            var introducedMethod = MethodDeclaration(
                List<AttributeListSyntax>(),
                modifiers,
                returnType,
                null,
                Identifier(
                    context.IntroductionNameProvider.GetOverrideName(
                        this.OverriddenDeclaration.DeclaringType,
                        this.ParentAdvice.AspectLayerId,
                        this.OverriddenDeclaration ) ),
                context.SyntaxGenerator.TypeParameterList( this.OverriddenDeclaration ),
                context.SyntaxGenerator.ParameterList( this.OverriddenDeclaration ),
                context.SyntaxGenerator.ConstraintClauses( this.OverriddenDeclaration ),
                newMethodBody,
                null );

            return new[]
            {
                new IntroducedMember(
                    this,
                    introducedMethod,
                    this.ParentAdvice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.OverriddenDeclaration )
            };
        }

        private ExpressionSyntax CreateInvocationExpression( SyntaxGenerationContext generationContext )
            => InvocationExpression(
                this.CreateMemberAccessExpression( AspectReferenceTargetKind.Self, generationContext ),
                ArgumentList(
                    SeparatedList(
                        this.OverriddenDeclaration.Parameters.Select(
                            p =>
                            {
                                var refKind = p.RefKind switch
                                {
                                    RefKind.None => default,
                                    RefKind.In => default,
                                    RefKind.Out => Token( SyntaxKind.OutKeyword ),
                                    RefKind.Ref => Token( SyntaxKind.RefKeyword ),
                                    _ => throw new AssertionFailedException()
                                };

                                return Argument( null, refKind, IdentifierName( p.Name ) );
                            } ) ) ) );
    }
}