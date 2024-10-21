// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;
using Metalama.Framework.Engine.Transformations;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Metalama.Framework.Tests.LinkerTests.Runner
{
    internal partial class LinkerTestInputBuilder
    {
        internal class InsertPositionRecord
        {
            /// <summary>
            /// The relation of the insertion.
            /// </summary>
            public InsertPositionRelation Relation { get; }

            /// <summary>
            /// The node ID of the node that is the target of the insertion in case this targets a source declaration.
            /// </summary>
            public string? NodeId { get; }

            /// <summary>
            /// The target builder data in case this targets a built declaration.
            /// </summary>
            public NamedDeclarationBuilderData? BuilderData { get; }

            public InsertPositionRecord( InsertPositionRelation relation, string nodeId )
            {
                this.NodeId = nodeId;
                this.Relation = relation;
            }

            public InsertPositionRecord( InsertPositionRelation relation, NamedDeclarationBuilderData? builderData )
            {
                this.BuilderData = builderData;
                this.Relation = relation;
            }
        }
    }
}