// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking
{
    internal struct LinkerInsertedStatement
    {
        public ITransformation ParentTransformation { get; }

        /// <summary>
        /// Gets a node of the original compilation (Block or EqualsValueClause) into which the new node will be inserted.
        /// </summary>
        public SyntaxNode? TargetNode { get; }

        /// <summary>
        /// Gets a member introduction into which the new node will be inserted.
        /// </summary>
        public IMemberIntroduction? TargetIntroduction { get; }

        public InsertedStatementPosition Position { get; }

        public SyntaxNode? Argument { get; }

        public LinkerInsertedStatement(
            ITransformation parentTransformation,
            SyntaxNode targetNode,
            InsertedStatementPosition position,
            SyntaxNode? argument )
        {
            this.ParentTransformation = parentTransformation;
            this.TargetNode = targetNode;
            this.TargetIntroduction = null;
            this.Position = position;
            this.Argument = argument;
        }

        public LinkerInsertedStatement(
            ITransformation parentTransformation,
            IMemberIntroduction targetIntroduction,
            InsertedStatementPosition position,
            SyntaxNode? argument )
        {
            this.ParentTransformation = parentTransformation;
            this.TargetNode = null;
            this.TargetIntroduction = targetIntroduction;
            this.Position = position;
            this.Argument = argument;
        }
    }
}