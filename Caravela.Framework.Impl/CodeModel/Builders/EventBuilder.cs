// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Builders;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class EventBuilder : MemberBuilder, IEventBuilder
    {
        private readonly bool _isEventField;

        public EventBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool isEventField )
            : base( parentAdvice, targetType, name )
        {
            this._isEventField = isEventField;
            this.EventType = (INamedType) targetType.Compilation.TypeFactory.GetTypeByReflectionType( typeof(EventHandler) );
        }

        public INamedType EventType { get; set; }

        public IMethod Signature => this.EventType.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethodBuilder Adder => new AccessorBuilder( this, MethodKind.EventAdd );

        [Memo]
        public IMethodBuilder Remover => new AccessorBuilder( this, MethodKind.EventRemove );

        public IMethodBuilder? Raiser => null;

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers => new InvokerFactory<IEventInvoker>( order => new EventInvoker( this, order ), false );

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                this.IsEventField()
                    ? ((MemberDeclarationSyntax?) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration()?.Parent?.Parent).AssertNotNull()
                    : ((MemberDeclarationSyntax?) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration()).AssertNotNull() );

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        INamedType IEvent.EventType => this.EventType;

        IMethod IEvent.Adder => this.Adder;

        IMethod IEvent.Remover => this.Remover;

        IMethod? IEvent.Raiser => this.Raiser;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IEvent>();

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = LanguageServiceFactory.CSharpSyntaxGenerator;

            if ( this._isEventField && this.ExplicitInterfaceImplementations.Count > 0 )
            {
                throw new AssertionFailedException();
            }

            MemberDeclarationSyntax @event =
                this._isEventField && this.ExplicitInterfaceImplementations.Count == 0
                    ? EventFieldDeclaration(
                        List<AttributeListSyntax>(), // TODO: Attributes.
                        this.GetSyntaxModifierList(),
                        VariableDeclaration(
                            syntaxGenerator.TypeExpression( this.EventType.GetSymbol() ),
                            SeparatedList(
                                new[]
                                {
                                    VariableDeclarator( Identifier( this.Name ), null, null ) // TODO: Initializer.
                                } ) ) )
                    : EventDeclaration(
                        List<AttributeListSyntax>(), // TODO: Attributes.
                        this.GetSyntaxModifierList(),
                        syntaxGenerator.TypeExpression( this.EventType.GetSymbol() ),
                        this.ExplicitInterfaceImplementations.Count > 0
                            ? ExplicitInterfaceSpecifier(
                                (NameSyntax) syntaxGenerator.TypeExpression( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
                            : null,
                        Identifier( this.Name ),
                        GenerateAccessorList() );

            if ( this._isEventField && this.ExplicitInterfaceImplementations.Count > 0 )
            {
                // Add annotation to the explicit annotation that the linker should treat this an event field.
                @event = @event.WithLinkerDeclarationFlags( LinkerDeclarationFlags.EventField );
            }

            return new[] { new IntroducedMember( this, @event, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };

            AccessorListSyntax GenerateAccessorList()
            {
                switch (this.Adder, this.Remover)
                {
                    case (not null, not null):
                        return AccessorList(
                            List( new[] { GenerateAccessor( SyntaxKind.AddAccessorDeclaration ), GenerateAccessor( SyntaxKind.RemoveAccessorDeclaration ) } ) );

                    case (not null, null):
                        return AccessorList( List( new[] { GenerateAccessor( SyntaxKind.AddAccessorDeclaration ) } ) );

                    case (null, not null):
                        return AccessorList( List( new[] { GenerateAccessor( SyntaxKind.RemoveAccessorDeclaration ) } ) );

                    default:
                        throw new AssertionFailedException();
                }
            }

            AccessorDeclarationSyntax GenerateAccessor( SyntaxKind accessorDeclarationKind )
            {
                return
                    AccessorDeclaration(
                        accessorDeclarationKind,
                        List<AttributeListSyntax>(),
                        TokenList(),
                        Block(),
                        null );
            }
        }

        [return: RunTimeOnly]
        public EventInfo ToEventInfo()
        {
            throw new NotImplementedException();
        }

        public void SetExplicitInterfaceImplementation( IEvent interfaceEvent )
        {
            this.ExplicitInterfaceImplementations = new[] { interfaceEvent };
        }

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;
    }
}