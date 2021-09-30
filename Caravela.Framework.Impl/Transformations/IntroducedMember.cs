// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents a member to be introduced in a type and encapsulates the information needed by the <see cref="AspectLinker"/>
    /// to perform the linking.
    /// </summary>
    internal class IntroducedMember : IComparable<IntroducedMember>
    {
        private static readonly ImmutableDictionary<DeclarationKind, int> _orderedDeclarationKinds = new Dictionary<DeclarationKind, int>()
        {
            { DeclarationKind.Field, 0 },
            { DeclarationKind.Constructor, 1 },
            { DeclarationKind.Property, 2 },
            { DeclarationKind.Method, 3 },
            { DeclarationKind.Event, 4 },
            { DeclarationKind.NamedType, 5 }
        }.ToImmutableDictionary();

        /// <summary>
        /// Gets the kind of declaration (for sorting only).
        /// </summary>
        public DeclarationKind DeclarationKind { get; }

        /// <summary>
        /// Gets the <see cref="IMemberIntroduction" /> that created this object.
        /// </summary>
        public IMemberIntroduction Introduction { get; }

        /// <summary>
        /// Gets the syntax of the introduced member.
        /// </summary>
        public MemberDeclarationSyntax Syntax { get; }

        /// <summary>
        /// Gets the <see cref="AspectLayerId"/> that emitted the current <see cref="IntroducedMember"/>.
        /// </summary>
        public AspectLayerId AspectLayerId { get; }

        /// <summary>
        /// Gets the semantic of the introduced member as supported by the linker.
        /// </summary>
        public IntroducedMemberSemantic Semantic { get; }

        /// <summary>
        /// Gets the declaration (overriden or introduced) that corresponds to the current <see cref="IntroducedMember"/>.
        /// This is used to associate diagnostic suppressions to the introduced member. If <c>null</c>, diagnostics
        /// are not suppressed from the introduced member.
        /// </summary>
        public IMember? Declaration { get; }

        public IntroducedMember(
            MemberBuilder introduction,
            MemberDeclarationSyntax syntax,
            AspectLayerId aspectLayerId,
            IntroducedMemberSemantic semantic,
            IMember? declaration ) : this(
            introduction,
            introduction.DeclarationKind,
            syntax,
            aspectLayerId,
            semantic,
            declaration ) { }

        public IntroducedMember(
            OverriddenMember introduction,
            MemberDeclarationSyntax syntax,
            AspectLayerId aspectLayerId,
            IntroducedMemberSemantic semantic,
            IMember? declaration ) : this(
            introduction,
            introduction.OverriddenDeclaration.DeclarationKind,
            syntax,
            aspectLayerId,
            semantic,
            declaration ) { }

        protected IntroducedMember(
            IntroducedMember prototype,
            MemberDeclarationSyntax syntax ) : this(
            prototype.Introduction,
            prototype.DeclarationKind,
            syntax,
            prototype.AspectLayerId,
            prototype.Semantic,
            prototype.Declaration ) { }

        internal IntroducedMember(
            IMemberIntroduction introduction,
            DeclarationKind kind,
            MemberDeclarationSyntax syntax,
            AspectLayerId aspectLayerId,
            IntroducedMemberSemantic semantic,
            IMember? declaration )
        {
            this.Introduction = introduction;
            this.Syntax = syntax.NormalizeWhitespace();
            this.AspectLayerId = aspectLayerId;
            this.Semantic = semantic;
            this.Declaration = declaration;
            this.DeclarationKind = kind;
        }

        public override string ToString() => this.Introduction.ToString();

        public int CompareTo( IntroducedMember? other )
        {
            if ( other == null )
            {
                return 1;
            }

            var declaration = this.Declaration ?? this.Introduction as IMember;

            if ( declaration == null && this.Introduction is IOverriddenDeclaration overridden )
            {
                declaration = (IMember) overridden.OverriddenDeclaration;
            }

            if ( declaration == null )
            {
                throw new AssertionFailedException( "Dont know how to sort." );
            }

            var kindComparison = GetKindOrder( this.DeclarationKind ).CompareTo( GetKindOrder( other.DeclarationKind ) );

            if ( kindComparison != 0 )
            {
                return kindComparison;
            }

            var nameComparison = string.CompareOrdinal( declaration?.Name, other.Declaration?.Name );

            if ( nameComparison != 0 )
            {
                return nameComparison;
            }

            if ( declaration is IMethod )
            {
                // Sort by signature.
                var signatureComparison = string.CompareOrdinal(
                    declaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimEnd( "" ),
                    other.Declaration?.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimEnd( "" ) );

                if ( signatureComparison != 0 )
                {
                    return signatureComparison;
                }
            }

            var typeComparison = GetTypeOrder( this.Introduction ).CompareTo( GetTypeOrder( other.Introduction ) );

            if ( typeComparison != 0 )
            {
                return typeComparison;
            }

            return 0;

            static int GetKindOrder( DeclarationKind kind ) => _orderedDeclarationKinds.TryGetValue( kind, out var order ) ? order : 10;

            static int GetTypeOrder( IMemberIntroduction introduction ) => introduction is IOverriddenDeclaration ? 0 : 1;
        }
    }
}