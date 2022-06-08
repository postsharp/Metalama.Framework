// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class EventBuilder : MemberBuilder, IEventBuilder, IEventImpl
    {
        private readonly bool _isEventField;

        public EventBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool isEventField,
            IObjectReader tags )
            : base( parentAdvice, targetType, tags )
        {
            this.Name = name;
            this._isEventField = isEventField;
            this.Type = (INamedType) targetType.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(EventHandler) );
        }

        public override string Name { get; set; }

        public override bool IsImplicit => false;

        public INamedType Type { get; set; }

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethodBuilder AddMethod => new AccessorBuilder( this, MethodKind.EventAdd, this._isEventField );

        [Memo]
        public IMethodBuilder RemoveMethod => new AccessorBuilder( this, MethodKind.EventRemove, this._isEventField );

        public IMethodBuilder? RaiseMethod => null;

        [Memo]
        public IInvokerFactory<IEventInvoker> Invokers
            => new InvokerFactory<IEventInvoker>(
                ( order, invokerOperator ) => new EventInvoker( this, order, invokerOperator ),
                this.OverriddenEvent != null );

        public IEvent? OverriddenEvent { get; set; }

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        INamedType IEvent.Type => this.Type;

        IMethod IEvent.AddMethod => this.AddMethod;

        IMethod IEvent.RemoveMethod => this.RemoveMethod;

        IMethod? IEvent.RaiseMethod => this.RaiseMethod;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IEvent>();

        public IExpression? InitializerExpression { get; set; }

        public TemplateMember<IEvent> InitializerTemplate { get; set; }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            _ = this.GetInitializerExpressionOrMethod(
                context,
                this.Type,
                this.InitializerExpression,
                this.InitializerTemplate,
                out var initializerExpression,
                out var initializerMethod );

            Invariant.Assert( !(!this._isEventField && initializerExpression != null) );

            MemberDeclarationSyntax @event =
                this._isEventField && this.ExplicitInterfaceImplementations.Count == 0
                    ? EventFieldDeclaration(
                        this.GetAttributeLists( context.SyntaxGenerationContext ),
                        this.GetSyntaxModifierList(),
                        VariableDeclaration(
                            syntaxGenerator.Type( this.Type.GetSymbol() ),
                            SeparatedList(
                                new[]
                                {
                                    VariableDeclarator(
                                        Identifier( this.Name ),
                                        null,
                                        initializerExpression != null
                                            ? EqualsValueClause( initializerExpression )
                                            : null ) // TODO: Initializer.
                                } ) ) )
                    : EventDeclaration(
                        this.GetAttributeLists( context.SyntaxGenerationContext ),
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

            if ( initializerMethod != null )
            {
                return new[]
                {
                    new IntroducedMember( this, @event, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ),
                    new IntroducedMember( this, initializerMethod, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.InitializerMethod, this )
                };
            }
            else
            {
                return new[] { new IntroducedMember( this, @event, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
            }

            AccessorListSyntax GenerateAccessorList()
            {
                switch (Adder: this.AddMethod, Remover: this.RemoveMethod)
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

        public override void Freeze()
        {
            base.Freeze();

            ((DeclarationBuilder?) this.AddMethod)?.Freeze();
            ((DeclarationBuilder?) this.RemoveMethod)?.Freeze();
        }
    }
}