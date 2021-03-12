// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.UnitTests.Linker.Helpers
{

    public partial class LinkerTestBase
    {
        /// <summary>
        /// Helper attribute where we store ids of syntax nodes representing code elements which can only be resolved in the processed compilation.
        /// </summary>
        public interface ITestTransformation
        {
            /// <summary>
            /// Id of the containing element, which will be present as an annotation in the syntax tree.
            /// </summary>
            string ContainingNodeId { get; }

            /// <summary>
            /// Id of the insert position node, which will be present as an annotation in the syntax tree.
            /// </summary>
            string InsertPositionNodeId { get; }

            /// <summary>
            /// Name of the overridden element. An element with the same signature needs to be found on the same type.
            /// </summary>
            string? OverriddenElementName { get; }

            /// <summary>
            /// Name of the introduced element.
            /// </summary>
            string? IntroducedElementName { get; }

            /// <summary>
            /// Syntax node that will appear in introduced member.
            /// </summary>
            string SymbolHelperNodeId { get; }
        }
    }
}
