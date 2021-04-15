// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Linking
{
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
            : base( original.Introductor, linkerAnnotatedSyntax, original.AspectLayerId, original.Semantic, original.LinkerOptions )
        {
            this.LinkerNodeId = linkerNodeId;
        }
    }
}