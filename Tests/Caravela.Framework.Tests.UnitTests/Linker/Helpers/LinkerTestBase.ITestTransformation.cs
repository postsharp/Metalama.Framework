// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Tests.UnitTests.Linker.Helpers
{
    public partial class LinkerTestBase
    {
        /// <summary>
        /// Helper attribute where we store ids of syntax nodes representing declarations which can only be resolved in the processed compilation.
        /// </summary>
        public interface ITestTransformation
        {
            /// <summary>
            /// Gets the id of the containing element, which will be present as an annotation in the syntax tree.
            /// </summary>
            string ContainingNodeId { get; }

            /// <summary>
            /// Gets the id of the insert position node, which will be present as an annotation in the syntax tree.
            /// </summary>
            string InsertPositionNodeId { get; }

            /// <summary>
            /// Gets the name of the overridden element. An element with the same signature needs to be found on the same type.
            /// </summary>
            string? OverriddenDeclarationName { get; }

            /// <summary>
            /// Gets the name of the introduced element.
            /// </summary>
            string? IntroducedElementName { get; }

            /// <summary>
            /// Gets the syntax node that will appear in introduced member.
            /// </summary>
            string SymbolHelperNodeId { get; }
        }
    }
}