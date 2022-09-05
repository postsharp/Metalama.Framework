// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        public IIntroduceMemberTransformation? TargetIntroduction { get; }

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
            StatementSyntax statement,
            IDeclaration contextDeclaration )
        {
            this.ParentTransformation = parentTransformation;
            this.TargetNode = targetNode;
            this.TargetIntroduction = null;
            this.Statement = statement;
            this.ContextDeclaration = contextDeclaration;
        }

        public LinkerInsertedStatement(
            ITransformation parentTransformation,
            IIntroduceMemberTransformation targetIntroduction,
            StatementSyntax statement,
            IDeclaration contextDeclaration )
        {
            this.ParentTransformation = parentTransformation;
            this.TargetNode = null;
            this.TargetIntroduction = targetIntroduction;
            this.Statement = statement;
            this.ContextDeclaration = contextDeclaration;
        }
    }
}