﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.AdviceImpl.Introduction;

internal sealed class IntroduceEventTransformation : IntroduceMemberTransformation<EventBuilderData>
{
    public IntroduceEventTransformation( AdviceInfo advice, EventBuilderData introducedDeclaration ) : base( advice, introducedDeclaration ) { }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;
        var eventBuilder = this.BuilderData.ToRef().GetTarget();

        _ = AdviceSyntaxGenerator.GetInitializerExpressionOrMethod(
            eventBuilder,
            this.ParentAdvice,
            context,
            eventBuilder.Type,
            this.BuilderData.InitializerExpression,
            this.BuilderData.InitializerTemplate,
            this.BuilderData.InitializerTags,
            out var initializerExpression,
            out var initializerMethod );

        var isEventField = this.BuilderData.IsEventField;
        Invariant.Assert( !(isEventField == false && initializerExpression != null) );

        // TODO: This should be handled by the linker.
        // If we are introducing a field into a struct in C# 10, it must have an explicit default value.
        if ( initializerExpression == null
             && isEventField
             && eventBuilder is { DeclaringType.TypeKind: TypeKind.Struct or TypeKind.RecordStruct }
             && context.SyntaxGenerationContext.RequiresStructFieldInitialization )
        {
            initializerExpression = SyntaxFactoryEx.Default;
        }

        // TODO: If the user adds (different) attributes to event field's accessors, we cannot use event fields.

        MemberDeclarationSyntax @event =
            isEventField && eventBuilder is { ExplicitInterfaceImplementations.Count: 0 }
                ? EventFieldDeclaration(
                    AdviceSyntaxGenerator.GetAttributeLists( eventBuilder, context ).AddRange( GetAdditionalAttributeListsForEventField() ),
                    eventBuilder.GetSyntaxModifierList(),
                    Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( ElasticSpace ) ),
                    VariableDeclaration(
                        syntaxGenerator.Type( eventBuilder.Type )
                            .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                        SeparatedList(
                        [
                            VariableDeclarator(
                                Identifier( TriviaList(), eventBuilder.Name, TriviaList( ElasticSpace ) ),
                                null,
                                initializerExpression != null
                                    ? EqualsValueClause( initializerExpression )
                                    : null ) // TODO: Initializer.
                        ] ) ),
                    Token( SyntaxKind.SemicolonToken ) )
                : EventDeclaration(
                    AdviceSyntaxGenerator.GetAttributeLists( eventBuilder, context ),
                    eventBuilder.GetSyntaxModifierList(),
                    Token( TriviaList(), SyntaxKind.EventKeyword, TriviaList( ElasticSpace ) ),
                    syntaxGenerator.Type( eventBuilder.Type )
                        .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    eventBuilder.ExplicitInterfaceImplementations.Count > 0
                        ? ExplicitInterfaceSpecifier(
                                (NameSyntax) syntaxGenerator.Type( eventBuilder.ExplicitInterfaceImplementations.Single().DeclaringType ) )
                            .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options )
                        : null,
                    eventBuilder.GetCleanName(),
                    GenerateAccessorList(),
                    default );

        if ( isEventField && eventBuilder is { ExplicitInterfaceImplementations.Count: > 0 } )
        {
            // Add annotation to the explicit annotation that the linker should treat this an event field.
            if ( initializerExpression != null )
            {
                @event = @event.WithLinkerDeclarationFlags(
                    AspectLinkerDeclarationFlags.EventField | AspectLinkerDeclarationFlags.HasHiddenInitializerExpression );
            }
            else
            {
                @event = @event.WithLinkerDeclarationFlags( AspectLinkerDeclarationFlags.EventField );
            }
        }

        if ( initializerMethod != null )
        {
            return
            [
                new InjectedMember( this, @event, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() ),
                new InjectedMember(
                    this,
                    initializerMethod,
                    this.AspectLayerId,
                    InjectedMemberSemantic.InitializerMethod,
                    this.BuilderData.ToRef() )
            ];
        }
        else
        {
            return [new InjectedMember( this, @event, this.AspectLayerId, InjectedMemberSemantic.Introduction, this.BuilderData.ToRef() )];
        }

        AccessorListSyntax GenerateAccessorList()
        {
            switch (Adder: eventBuilder.AddMethod, Remover: eventBuilder.RemoveMethod)
            {
                case (not null, not null):
                    return AccessorList(
                        List(
                        [
                            GenerateAccessor( eventBuilder.AddMethod, SyntaxKind.AddAccessorDeclaration ),
                            GenerateAccessor( eventBuilder.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration )
                        ] ) );

                case (not null, null):
                    return AccessorList( List( [GenerateAccessor( eventBuilder.AddMethod, SyntaxKind.AddAccessorDeclaration )] ) );

                case (null, not null):
                    return AccessorList( List( [GenerateAccessor( eventBuilder.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration )] ) );

                default:
                    throw new AssertionFailedException( "Both accessors are null." );
            }
        }

        AccessorDeclarationSyntax GenerateAccessor( IMethod accessor, SyntaxKind accessorDeclarationKind )
        {
            var attributes = AdviceSyntaxGenerator.GetAttributeLists( accessor, context );

            var block =
                accessor switch
                {
                    // Special case - explicit interface implementation event field with initialized.
                    // Hide initializer expression into the single statement of the add.
                    { MethodKind: MethodKind.EventAdd } when isEventField && eventBuilder is { ExplicitInterfaceImplementations.Count: > 0 }
                                                                          && initializerExpression != null
                        => context.SyntaxGenerator.FormattedBlock(
                            ExpressionStatement(
                                context.AspectReferenceSyntaxProvider.GetEventFieldInitializerExpression(
                                    syntaxGenerator.Type( eventBuilder.Type ),
                                    initializerExpression ) ) ),
                    _ => context.SyntaxGenerator.FormattedBlock()
                };

            return
                AccessorDeclaration(
                    accessorDeclarationKind,
                    attributes,
                    TokenList(),
                    block,
                    null );
        }

        IEnumerable<AttributeListSyntax> GetAdditionalAttributeListsForEventField()
        {
            var attributes = new List<AttributeListSyntax>();

            foreach ( var attribute in this.BuilderData.FieldAttributes )
            {
                attributes.Add(
                    AttributeList(
                        AttributeTargetSpecifier( Token( SyntaxKind.FieldKeyword ) ),
                        SingletonSeparatedList( context.SyntaxGenerator.Attribute( attribute ) ) ) );
            }

            // Here we take attributes only for add method because we presume it's the same.

            foreach ( var attribute in eventBuilder.AddMethod.Attributes )
            {
                attributes.Add(
                    AttributeList(
                        AttributeTargetSpecifier( Token( SyntaxKind.MethodKeyword ) ),
                        SingletonSeparatedList( context.SyntaxGenerator.Attribute( attribute ) ) ) );
            }

            return List( attributes );
        }
    }
}