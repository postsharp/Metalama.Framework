// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.Transformations;

internal class IntroduceEventTransformation : IntroduceMemberTransformation<EventBuilder>
{
    public IntroduceEventTransformation( Advice advice, EventBuilder introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMember> GetIntroducedMembers( MemberInjectionContext context )
    {
        var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;
        var eventBuilder = this.IntroducedDeclaration;

        _ = eventBuilder.GetInitializerExpressionOrMethod(
            this.ParentAdvice,
            context,
            eventBuilder.Type,
            eventBuilder.InitializerExpression,
            eventBuilder.InitializerTemplate,
            eventBuilder.InitializerTags,
            out var initializerExpression,
            out var initializerMethod );

        Invariant.Assert( !(!eventBuilder.IsEventField && initializerExpression != null) );

        MemberDeclarationSyntax @event =
            eventBuilder.IsEventField && eventBuilder.ExplicitInterfaceImplementations.Count == 0
                ? EventFieldDeclaration(
                    eventBuilder.GetAttributeLists( context ),
                    eventBuilder.GetSyntaxModifierList(),
                    Token(TriviaList(),SyntaxKind.EventKeyword, TriviaList(ElasticSpace)),
                    VariableDeclaration(
                        syntaxGenerator.Type( eventBuilder.Type.GetSymbol() ),
                        SeparatedList(
                            new[]
                            {
                                SyntaxFactory.VariableDeclarator(
                                    Identifier( eventBuilder.Name ),
                                    null,
                                    initializerExpression != null
                                        ? SyntaxFactory.EqualsValueClause( initializerExpression )
                                        : null ) // TODO: Initializer.
                            } ) ),
                    Token(SyntaxKind.SemicolonToken) )
                : EventDeclaration(
                    eventBuilder.GetAttributeLists( context ),
                    eventBuilder.GetSyntaxModifierList(),
                    Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( ElasticSpace ) ),
                    syntaxGenerator.Type( eventBuilder.Type.GetSymbol() ).WithTrailingTrivia( Space ),
                    eventBuilder.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier(
                            (NameSyntax) syntaxGenerator.Type( eventBuilder.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) ).WithTrailingTrivia( Space )
                        : null,
                    this.IntroducedDeclaration.GetCleanName(),
                    GenerateAccessorList() );

        if ( eventBuilder.IsEventField && eventBuilder.ExplicitInterfaceImplementations.Count > 0 )
        {
            // Add annotation to the explicit annotation that the linker should treat this an event field.
            @event = @event.WithLinkerDeclarationFlags( AspectLinkerDeclarationFlags.EventField );
        }

        if ( initializerMethod != null )
        {
            return new[]
            {
                new InjectedMember( this, @event, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, eventBuilder ),
                new InjectedMember(
                    this,
                    initializerMethod,
                    this.ParentAdvice.AspectLayerId,
                    IntroducedMemberSemantic.InitializerMethod,
                    eventBuilder )
            };
        }
        else
        {
            return new[] { new InjectedMember( this, @event, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, eventBuilder ) };
        }

        AccessorListSyntax GenerateAccessorList()
        {
            switch (Adder: eventBuilder.AddMethod, Remover: eventBuilder.RemoveMethod)
            {
                case (not null, not null):
                    return AccessorList(
                        List(
                            new[]
                            {
                                GenerateAccessor( eventBuilder.AddMethod, SyntaxKind.AddAccessorDeclaration ),
                                GenerateAccessor( eventBuilder.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration )
                            } ) );

                case (not null, null):
                    return AccessorList(
                        List( new[] { GenerateAccessor( eventBuilder.AddMethod, SyntaxKind.AddAccessorDeclaration ) } ) );

                case (null, not null):
                    return AccessorList(
                        List( new[] { GenerateAccessor( eventBuilder.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration ) } ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        AccessorDeclarationSyntax GenerateAccessor( IMethod accessor, SyntaxKind accessorDeclarationKind )
        {
            var attributes = eventBuilder.GetAttributeLists( context, accessor );

            var block =
                accessor switch
                {
                    // Special case - explicit interface implementation event field with initialized.
                    // Hide initializer expression into the single statement of the add.
                    { MethodKind: MethodKind.EventAdd } when eventBuilder.IsEventField && eventBuilder.ExplicitInterfaceImplementations.Count > 0
                                                                               && initializerExpression != null
                        => SyntaxFactoryEx.FormattedBlock(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName( Identifier( TriviaList(), SyntaxKind.UnderscoreToken, "_", "_", TriviaList() ) ),
                                    initializerExpression ) ) ),
                    _ => SyntaxFactoryEx.FormattedBlock()
                };

            return
                AccessorDeclaration(
                    accessorDeclarationKind,
                    attributes,
                    TokenList(),
                    block,
                    null );
        }
    }
}