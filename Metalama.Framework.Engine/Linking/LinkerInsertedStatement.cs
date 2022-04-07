﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        /// <summary>
        /// Gets the position of the inserted statement.
        /// </summary>
        public InsertedStatementPosition Position { get; }

        /// <summary>
        /// Gets the inserted statement.
        /// </summary>
        public StatementSyntax Statement { get; }

        /// <summary>
        /// Gets the declaration to which the statement relates to. Statements are first ordered by hierarchy and then by aspect order.
        /// </summary>
        public IDeclaration ContextDeclaration { get; }

        public LinkerInsertedStatement(
            ITransformation parentTransformation,
            SyntaxNode targetNode,
            InsertedStatementPosition position,
            StatementSyntax statement, 
            IDeclaration contextDeclaration )
        {
            this.ParentTransformation = parentTransformation;
            this.TargetNode = targetNode;
            this.TargetIntroduction = null;
            this.Position = position;
            this.Statement = statement;
            this.ContextDeclaration = contextDeclaration;
        }

        public LinkerInsertedStatement(
            ITransformation parentTransformation,
            IMemberIntroduction targetIntroduction,
            InsertedStatementPosition position,
            StatementSyntax statement,
            IDeclaration contextDeclaration )
        {
            this.ParentTransformation = parentTransformation;
            this.TargetNode = null;
            this.TargetIntroduction = targetIntroduction;
            this.Position = position;
            this.Statement = statement;
            this.ContextDeclaration = contextDeclaration;
        }
    }
}