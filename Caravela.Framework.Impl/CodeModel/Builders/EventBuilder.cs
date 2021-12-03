// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class EventBuilder : MemberBuilder, IEventBuilder, IEventImpl
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
            this.Type = (INamedType) targetType.Compilation.TypeFactory.GetTypeByReflectionType( typeof( EventHandler ) );
        }

        public INamedType Type { get; set; }

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethodBuilder AddMethod => new AccessorBuilder( this, MethodKind.EventAdd );

        [Memo]
        public IMethodBuilder RemoveMethod => new AccessorBuilder( this, MethodKind.EventRemove );

        public IMethodBuilder? RaiseMethod => null;

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers
            => new InvokerFactory<IEventInvoker>(
                ( order, invokerOperator ) => new EventInvoker( this, order, invokerOperator ),
                this.OverriddenEvent != null );

        public IEvent? OverriddenEvent { get; set; }

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                this.IsEventField()
                    ? ((MemberDeclarationSyntax?) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration()?.Parent?.Parent).AssertNotNull()
                    : ((MemberDeclarationSyntax?) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration()).AssertNotNull() );

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        INamedType IEvent.Type => this.Type;

        IMethod IEvent.AddMethod => this.AddMethod;

        IMethod IEvent.RemoveMethod => this.RemoveMethod;

        IMethod? IEvent.RaiseMethod => this.RaiseMethod;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IEvent>();

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            MemberDeclarationSyntax @event =
                this._isEventField && this.ExplicitInterfaceImplementations.Count == 0
                    ? EventFieldDeclaration(
                        List<AttributeListSyntax>(), // TODO: Attributes.
                        this.GetSyntaxModifierList(),
                        VariableDeclaration(
                            syntaxGenerator.Type( this.Type.GetSymbol() ),
                            SeparatedList(
                                new[]
                                {
                                    VariableDeclarator( Identifier( this.Name ), null, null ) // TODO: Initializer.
                                } ) ) )
                    : EventDeclaration(
                        List<AttributeListSyntax>(), // TODO: Attributes.
                        this.GetSyntaxModifierList(),
                        syntaxGenerator.Type( this.Type.GetSymbol() ),
                        this.ExplicitInterfaceImplementations.Count > 0
                            ? ExplicitInterfaceSpecifier(
                                (NameSyntax) syntaxGenerator.Type( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() ) )
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
                switch (Adder: this.AddMethod, Remover: this.RemoveMethod)
                {
                    case (not null, not null ):
                        return AccessorList(
                            List( new[] { GenerateAccessor( SyntaxKind.AddAccessorDeclaration ), GenerateAccessor( SyntaxKind.RemoveAccessorDeclaration ) } ) );

                    case (not null, null ):
                        return AccessorList( List( new[] { GenerateAccessor( SyntaxKind.AddAccessorDeclaration ) } ) );

                    case (null, not null ):
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

        public EventInfo ToEventInfo() => throw new NotImplementedException();

        public void SetExplicitInterfaceImplementation( IEvent interfaceEvent ) => this.ExplicitInterfaceImplementations = new[] { interfaceEvent };

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override IMember? OverriddenMember => (IMemberImpl?) this.OverriddenEvent;

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.EventAdd => this.AddMethod,
                MethodKind.EventRaise => this.RaiseMethod,
                MethodKind.EventRemove => this.RemoveMethod,
                _ => null
            };

        public IEnumerable<IMethod> Accessors
        {
            get
            {
                yield return this.AddMethod;
                yield return this.RemoveMethod;

                if ( this.RaiseMethod != null )
                {
                    yield return this.RaiseMethod;
                }
            }
        }

        IType IHasType.Type => this.Type;
    }
}