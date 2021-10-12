// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.Linking;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents a member to be introduced in a type and encapsulates the information needed by the <see cref="AspectLinker"/>
    /// to perform the linking.
    /// </summary>
    internal class IntroducedMember
    {
        public DeclarationKind Kind { get; }

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
            prototype.Kind,
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
            this.Kind = kind;
        }

        public override string ToString() => this.Introduction.ToString();

    }
}