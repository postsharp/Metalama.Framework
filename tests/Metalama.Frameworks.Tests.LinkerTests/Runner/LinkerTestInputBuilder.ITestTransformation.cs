// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Transformations;

namespace Metalama.Framework.Tests.LinkerTests.Runner
{
    internal partial class LinkerTestInputBuilder
    {
        /// <summary>
        /// Helper attribute where we store ids of syntax nodes representing declarations which can only be resolved in the processed compilation.
        /// </summary>
        internal interface ITestTransformation
        {
            /// <summary>
            /// Gets the id of the containing element, which will be present as an annotation in the syntax tree.
            /// </summary>
            string ContainingNodeId { get; }

            /// <summary>
            /// Gets the id of the insert position node, which will be present as an annotation in the syntax tree and specifies the insert position.
            /// </summary>
            string? InsertPositionNodeId { get; }

            /// <summary>
            /// Gets the relation of the insert position to the specified insert position node.
            /// </summary>
            InsertPositionRelation InsertPositionRelation { get; }

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

            /// <summary>
            /// Gets the name of the member replaced by this declaration.
            /// </summary>
            string ReplacedElementName { get; }

            ITransformation ActualTransformation { get; }
        }
    }
}