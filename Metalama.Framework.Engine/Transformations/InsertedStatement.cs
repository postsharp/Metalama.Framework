// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        /// Gets the operand syntax.
        /// </summary>
        public StatementSyntax Statement { get; }

        /// <summary>
        /// Gets the declaration to which the statement relates to. Statements are first ordered by statement kind, then by hierarchy and then by aspect order.
        /// </summary>
        public IDeclaration ContextDeclaration { get; }

        public InsertedStatementKind Kind { get; }

        public InsertedStatement( StatementSyntax newNode, IDeclaration contextDeclaration, InsertedStatementKind kind = InsertedStatementKind.Regular )
        {
            this.Statement = newNode;
            this.ContextDeclaration = contextDeclaration;
            this.Kind = kind;
        }
    }
}