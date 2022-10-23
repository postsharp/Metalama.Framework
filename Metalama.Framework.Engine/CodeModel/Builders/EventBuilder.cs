// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Metalama.Framework.Code.MethodKind;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class EventBuilder : MemberBuilder, IEventBuilder, IEventImpl
    {
        private readonly IObjectReader _initializerTags;

        public bool IsEventField { get; }

        public EventBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool isEventField,
            IObjectReader initializerTags )
            : base( parentAdvice, targetType, name )
        {
            this._initializerTags = initializerTags;
            this.IsEventField = isEventField;
            this.Type = (INamedType) targetType.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(EventHandler) );
        }

        public INamedType Type { get; set; }

        public IMethod Signature => this.Type.Methods.OfName( "Invoke" ).Single();

        [Memo]
        public IMethodBuilder AddMethod => new AccessorBuilder( this, MethodKind.EventAdd, this.IsEventField );

        [Memo]
        public IMethodBuilder RemoveMethod => new AccessorBuilder( this, MethodKind.EventRemove, this.IsEventField );

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

        public TemplateMember<IEvent>? InitializerTemplate { get; set; }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            _ = this.GetInitializerExpressionOrMethod(
                context,
                this.Type,
                this.InitializerExpression,
                this.InitializerTemplate,
                this._initializerTags,
                out var initializerExpression,
                out var initializerMethod );

            Invariant.Assert( !(!this.IsEventField && initializerExpression != null) );

            // TODO: This should be handled by the linker.
            // If we are introducing a field into a struct, it must have an explicit default value.
            if ( initializerExpression == null && this.IsEventField && this.DeclaringType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct )
            {
                initializerExpression = SyntaxFactoryEx.Default;
            }

            MemberDeclarationSyntax @event =
                this.IsEventField && this.ExplicitInterfaceImplementations.Count == 0
                    ? EventFieldDeclaration(
                        this.GetAttributeLists( context ),
                        this.GetSyntaxModifierList(),
                        Token( SyntaxKind.EventKeyword ).WithTrailingTrivia( Space ),
                        VariableDeclaration(
                            syntaxGenerator.Type( this.Type.GetSymbol() ).WithTrailingTrivia( Space ),
                            SeparatedList(
                                new[]
                                {
                                    VariableDeclarator(
                                        Identifier( this.Name ),
                                        null,
                                        initializerExpression != null
                                            ? EqualsValueClause( initializerExpression )
                                            : null ) // TODO: Initializer.
                                } ) ),
                        Token( SyntaxKind.SemicolonToken ) )
                    : EventDeclaration(
                        this.GetAttributeLists( context ),
                        this.GetSyntaxModifierList(),
                        Token( SyntaxKind.EventKeyword ).WithTrailingTrivia( Space ),
                        syntaxGenerator.Type( this.Type.GetSymbol() ).WithTrailingTrivia( Space ),
                        this.ExplicitInterfaceImplementations.Count > 0
                            ? ExplicitInterfaceSpecifier(
                                (NameSyntax) syntaxGenerator.Type( this.ExplicitInterfaceImplementations[0].DeclaringType.GetSymbol() )
                                    .WithTrailingTrivia( Space ) )
                            : null!,
                        this.GetCleanName(),
                        GenerateAccessorList() );

            if ( this.IsEventField && this.ExplicitInterfaceImplementations.Count > 0 )
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
                            List(
                                new[]
                                {
                                    GenerateAccessor( this.AddMethod, SyntaxKind.AddAccessorDeclaration ),
                                    GenerateAccessor( this.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration )
                                } ) );

                    case (not null, null):
                        return AccessorList( List( new[] { GenerateAccessor( this.AddMethod, SyntaxKind.AddAccessorDeclaration ) } ) );

                    case (null, not null):
                        return AccessorList( List( new[] { GenerateAccessor( this.RemoveMethod, SyntaxKind.RemoveAccessorDeclaration ) } ) );

                    default:
                        throw new AssertionFailedException();
                }
            }

            AccessorDeclarationSyntax GenerateAccessor( IMethod accessor, SyntaxKind accessorDeclarationKind )
            {
                var attributes = this.GetAttributeLists( context, accessor );

                var block =
                    accessor switch
                    {
                        // Special case - explicit interface implementation event field with initialized.
                        // Hide initializer expression into the single statement of the add.
                        { MethodKind: MethodKind.EventAdd } when this.IsEventField && this.ExplicitInterfaceImplementations.Count > 0
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

        public EventInfo ToEventInfo() => CompileTimeEventInfo.Create( this );

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