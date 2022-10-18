// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Linking;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

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
                ? SyntaxFactory.EventFieldDeclaration(
                    eventBuilder.GetAttributeLists( context ),
                    eventBuilder.GetSyntaxModifierList(),
                    SyntaxFactory.VariableDeclaration(
                        syntaxGenerator.Type( eventBuilder.Type.GetSymbol() ),
                        SyntaxFactory.SeparatedList(
                            new[]
                            {
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier( eventBuilder.Name ),
                                    null,
                                    initializerExpression != null
                                        ? SyntaxFactory.EqualsValueClause( initializerExpression )
                                        : null ) // TODO: Initializer.
                            } ) ) )
                : SyntaxFactory.EventDeclaration(
                    eventBuilder.GetAttributeLists( context ),
                    eventBuilder.GetSyntaxModifierList(),
                    syntaxGenerator.Type( eventBuilder.Type.GetSymbol() ),
                    eventBuilder.ExplicitInterfaceImplementations.Count > 0
                        ? SyntaxFactory.ExplicitInterfaceSpecifier(
                            (NameSyntax) syntaxGenerator.Type( eventBuilder.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
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
                    return SyntaxFactory.AccessorList(
                        SyntaxFactory.List(
                            new[]
                            {
                                GenerateAccessor( eventBuilder.AddMethod, SyntaxKind.AddAccessorDeclaration ),
                                GenerateAccessor( eventBuilder.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration )
                            } ) );

                case (not null, null):
                    return SyntaxFactory.AccessorList(
                        SyntaxFactory.List( new[] { GenerateAccessor( eventBuilder.AddMethod, SyntaxKind.AddAccessorDeclaration ) } ) );

                case (null, not null):
                    return SyntaxFactory.AccessorList(
                        SyntaxFactory.List( new[] { GenerateAccessor( eventBuilder.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration ) } ) );

                default:
                    throw new AssertionFailedException();
            }
        }

        AccessorDeclarationSyntax GenerateAccessor( IMethod accessor, SyntaxKind accessorDeclarationKind )
        {
            var attributes = eventBuilder.GetAttributeLists( context, accessor );

            return
                SyntaxFactory.AccessorDeclaration(
                    accessorDeclarationKind,
                    attributes,
                    SyntaxFactory.TokenList(),
                    SyntaxFactory.Block(),
                    null );
        }
    }
}