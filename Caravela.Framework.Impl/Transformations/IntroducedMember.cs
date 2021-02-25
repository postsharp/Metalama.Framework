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

        public IntroducedMember( IMemberIntroduction introductor, MemberDeclarationSyntax syntax, AspectLayerId aspectLayerId, IntroducedMemberSemantic semantic )
        {
            this.Introductor = introductor;
            this.Syntax = syntax;
            this.AspectLayerId = aspectLayerId;
            this.Semantic = semantic;
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