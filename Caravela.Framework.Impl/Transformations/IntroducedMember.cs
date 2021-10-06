// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Accessibility = Caravela.Framework.Code.Accessibility;

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

        private static readonly ImmutableDictionary<Accessibility, int> _orderedAccessibilities = new Dictionary<Accessibility, int>()
        {
            { Accessibility.Public, 0 },
            { Accessibility.Protected, 1 },
            { Accessibility.ProtectedInternal, 2 },
            { Accessibility.Internal, 3 },
            { Accessibility.PrivateProtected, 4 },
            { Accessibility.Private, 5 }
        }.ToImmutableDictionary();

        private readonly DeclarationKind _declarationKind;

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
            prototype._declarationKind,
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
            this._declarationKind = kind;
        }

        public override string ToString() => this.Introduction.ToString();

        public int CompareTo( IntroducedMember? other )
        {
            if ( other == null )
            {
                return 1;
            }

            var declaration = GetDeclaration( this );
            var otherDeclaration = GetDeclaration( other );

            // Order by kind.
            var kindComparison = GetKindOrder( this._declarationKind ).CompareTo( GetKindOrder( other._declarationKind ) );

            if ( kindComparison != 0 )
            {
                return kindComparison;
            }

            // Order by name.
            var nameComparison = string.CompareOrdinal( declaration.Name, otherDeclaration.Name );

            if ( nameComparison != 0 )
            {
                return nameComparison;
            }

            // Order by signature.
            if ( declaration is IMethod )
            {
                var signatureComparison = string.CompareOrdinal(
                    declaration.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimEnd( "" ),
                    other.Declaration?.ToDisplayString( CodeDisplayFormat.MinimallyQualified ).TrimEnd( "" ) );

                if ( signatureComparison != 0 )
                {
                    return signatureComparison;
                }
            }

            // Order by accessibility.
            var accessibilityComparison =
                GetAccessibilityOrder( declaration.Accessibility ).CompareTo( GetAccessibilityOrder( otherDeclaration.Accessibility ) );

            if ( accessibilityComparison != 0 )
            {
                return accessibilityComparison;
            }

            // Order by implemented interface.
            var isExplicitInterfaceImplementationComparison =
                declaration.IsExplicitInterfaceImplementation.CompareTo( otherDeclaration.IsExplicitInterfaceImplementation );

            if ( isExplicitInterfaceImplementationComparison != 0 )
            {
                return -isExplicitInterfaceImplementationComparison;
            }
            else if ( declaration.IsExplicitInterfaceImplementation )
            {
                var interfaceComparison = string.Compare(
                    declaration.GetExplicitInterfaceImplementation().DeclaringType.FullName,
                    otherDeclaration.GetExplicitInterfaceImplementation().DeclaringType.FullName,
                    StringComparison.Ordinal );

                if ( interfaceComparison != 0 )
                {
                    return interfaceComparison;
                }
            }

            // Order by type of introduction.
            var typeComparison = GetTypeOrder( this.Introduction ).CompareTo( GetTypeOrder( other.Introduction ) );

            if ( typeComparison != 0 )
            {
                return typeComparison;
            }

            // It's ok if two instances are weakly ordered. Instances are then sorted by aspect layer, but this class does not
            // contain information about the ordering of aspect layers.

            return 0;
        }

        private static int GetKindOrder( DeclarationKind kind ) => _orderedDeclarationKinds.TryGetValue( kind, out var order ) ? order : 10;

        private static int GetAccessibilityOrder( Accessibility accessibility )
            => _orderedAccessibilities.TryGetValue( accessibility, out var order ) ? order : 10;

        private static int GetTypeOrder( IMemberIntroduction introduction ) => introduction is IOverriddenDeclaration ? 0 : 1;

        private static IMember GetDeclaration( IntroducedMember introducedMember )
        {
            var declaration = introducedMember.Declaration ?? introducedMember.Introduction as IMember;

            if ( declaration == null && introducedMember.Introduction is IOverriddenDeclaration overridden )
            {
                declaration = (IMember) overridden.OverriddenDeclaration;
            }

            if ( declaration == null )
            {
                throw new AssertionFailedException( "Dont know how to sort." );
            }

            return declaration;
        }
    }
}