// unset

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
        /// Gets the syntax of the introduced member.
        /// </summary>
        public MemberDeclarationSyntax Syntax { get; }

        /// <summary>
        /// Gets the <see cref="AspectPart"/> that emitted the current <see cref="IntroducedMember"/>.
        /// </summary>
        public AspectPartId AspectPart { get; }

        /// <summary>
        /// Gets the semantic of the introduced member as supported by the linker.
        /// </summary>
        public IntroducedMemberSemantic Semantic { get; }

        public IntroducedMember( MemberDeclarationSyntax syntax, AspectPartId aspectPart, IntroducedMemberSemantic semantic )
        {
            this.Syntax = syntax;
            this.AspectPart = aspectPart;
            this.Semantic = semantic;
        }
    }
}