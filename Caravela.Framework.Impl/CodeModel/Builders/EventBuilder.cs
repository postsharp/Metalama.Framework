// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class EventBuilder : MemberBuilder, IEventBuilder
    {
        private readonly bool _isEventField;

        public AspectLinkerOptions? LinkerOptions { get; }

        public EventBuilder(
            Advice parentAdvice,
            INamedType targetType,
            string name,
            bool isEventField,
            AspectLinkerOptions? linkerOptions )
            : base( parentAdvice, targetType, name )
        {
            this._isEventField = isEventField;
            this.LinkerOptions = linkerOptions;
            this.EventType = targetType.Compilation.TypeFactory.GetTypeByReflectionType( typeof( EventHandler ) );
        }

        public IType EventType { get; set; }

        public IEventInvocation Base => throw new NotImplementedException();

        [Memo]
        public IMethodBuilder Adder => new AccessorBuilder( this, MethodKind.EventAdd );

        [Memo]
        public IMethodBuilder Remover => new AccessorBuilder( this, MethodKind.EventRemove );

        public IMethodBuilder? Raiser => null;

        [Memo]
        public override MemberDeclarationSyntax InsertPositionNode
            => ((NamedType) this.DeclaringType).Symbol.DeclaringSyntaxReferences.Select( x => (TypeDeclarationSyntax) x.GetSyntax() ).FirstOrDefault();

        public override DeclarationKind DeclarationKind => DeclarationKind.Event;

        IType IEvent.EventType => this.EventType;

        IMethod IEvent.Adder => this.Adder;

        IMethod IEvent.Remover => this.Remover;

        IMethod? IEvent.Raiser => this.Raiser;

        // TODO: When an interface is introduced, explicit implementation should appear here.
        public IReadOnlyList<IEvent> ExplicitInterfaceImplementations => Array.Empty<IEvent>();

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = this.Compilation.SyntaxGenerator;

            var @event =
                this._isEventField
                ? EventFieldDeclaration(
                    List<AttributeListSyntax>(), // TODO: Attributes.
                    GenerateModifierList(),
                    VariableDeclaration(
                        (TypeSyntax) syntaxGenerator.TypeExpression( this.EventType.GetSymbol() ),
                        SeparatedList(
                            new[]
                            {
                                VariableDeclarator( Identifier( this.Name ), null, null ), // TODO: Initializer.
                            } ) ) )
                : throw new NotImplementedException();


            return new[]
            {
                new IntroducedMember( this, @event, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this.LinkerOptions, this )
            };

            SyntaxTokenList GenerateModifierList()
            {
                // Modifiers for event.
                var tokens = new List<SyntaxToken>();

                this.Accessibility.AddTokens( tokens );

                if ( this.IsAbstract )
                {
                    tokens.Add( Token( SyntaxKind.AbstractKeyword ) );
                }

                if ( this.IsSealed )
                {
                    tokens.Add( Token( SyntaxKind.SealedKeyword ) );
                }

                if ( this.IsOverride )
                {
                    tokens.Add( Token( SyntaxKind.OverrideKeyword ) );
                }

                return TokenList( tokens );
            }
        }

        [return: RunTimeOnly]
        public EventInfo ToEventInfo()
        {
            throw new NotImplementedException();
        }
    }
}