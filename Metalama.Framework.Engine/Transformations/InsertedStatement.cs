// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a mark on a syntax node that will be a target of code transformations.
    /// </summary>
    internal readonly struct InsertedStatement
    {
        /// <summary>
        /// Gets the operator for this code transformation.
        /// </summary>
        public InsertedStatementPosition Position { get; }

        /// <summary>
        /// Gets the operand syntax.
        /// </summary>
        public StatementSyntax Statement { get; }

        /// <summary>
        /// Gets the declaration to which the statement relates to. Statements are first ordered by hierarchy and then by aspect order.
        /// </summary>
        public IDeclaration ContextDeclaration { get; }

        public InsertedStatement( InsertedStatementPosition position, StatementSyntax newNode, IDeclaration contextDeclaration )
        {
            this.Position = position;
            this.Statement = newNode;
            this.ContextDeclaration = contextDeclaration;
        }
    }
}