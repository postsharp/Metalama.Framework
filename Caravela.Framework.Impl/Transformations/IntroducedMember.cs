// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Linking;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents a member to be introduced in a type and encapsulates the information needed by the <see cref="AspectLinker"/>
    /// to perform the linking.
    /// </summary>
    internal class IntroducedMember
    {
        /// <summary>
        /// Gets the <see cref="IMemberIntroduction" /> that created this object.
        /// </summary>
        public IMemberIntroduction Introductor { get; }

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
        /// Gets options for the linker.
        /// </summary>
        public AspectLinkerOptions? LinkerOptions { get; }

        public IntroducedMember(
            IMemberIntroduction introductor,
            MemberDeclarationSyntax syntax,
            AspectLayerId aspectLayerId,
            IntroducedMemberSemantic semantic,
            AspectLinkerOptions? linkerOptions )
        {
            this.Introductor = introductor;
            this.Syntax = syntax;
            this.AspectLayerId = aspectLayerId;
            this.Semantic = semantic;
            this.LinkerOptions = linkerOptions;
        }

        /// <summary>
        /// Gets introduced member with replaced syntax.
        /// </summary>
        /// <param name="syntax">Syntax to be used.</param>
        /// <returns>A new instance with specified syntax.</returns>
        public IntroducedMember WithSyntax( MemberDeclarationSyntax syntax )
        {
            return new( this.Introductor, syntax, this.AspectLayerId, this.Semantic, this.LinkerOptions );
        }
    }

    internal enum IntroducedMemberSemantic
    {
        Introduction,
        MethodOverride,
        GetterOverride,
        SetterOverride,
        AdderOverride,
        RemoverOverride,
        RaiserOverride
    }
}