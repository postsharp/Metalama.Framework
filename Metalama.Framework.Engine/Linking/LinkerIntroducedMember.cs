// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking
{
    // TODO: the use of LinkerIntroducedMember is a smell/hack.

    /// <summary>
    /// Extended <see cref="IntroducedMember"/> used by <see cref="AspectLinker"/>.
    /// </summary>
    internal class LinkerIntroducedMember : IntroducedMember
    {
        /// <summary>
        /// Gets id, which can be used to identify syntax node with the original transformation.
        /// </summary>
        public string LinkerNodeId { get; }

        public LinkerIntroducedMember( string linkerNodeId, MemberDeclarationSyntax linkerAnnotatedSyntax, IntroducedMember original )
            : base( original, linkerAnnotatedSyntax )
        {
            this.LinkerNodeId = linkerNodeId;
        }
    }
}