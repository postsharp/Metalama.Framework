// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking
{
    internal readonly struct LinkerInsertedStatement
    {
        public ITransformation ParentTransformation { get; }

        /// <summary>
        /// Gets the inserted statement.
        /// </summary>
        public StatementSyntax Statement { get; }

        /// <summary>
        /// Gets the declaration to which the statement relates to. Statements are first ordered by kind, then by hierarchy and then by aspect order.
        /// </summary>
        public IDeclaration ContextDeclaration { get; }

        public InsertedStatementKind Kind { get; }

        public LinkerInsertedStatement(
            ITransformation parentTransformation,
            StatementSyntax statement,
            IDeclaration contextDeclaration,
            InsertedStatementKind kind )
        {
            this.ParentTransformation = parentTransformation;
            this.Statement = statement;
            this.ContextDeclaration = contextDeclaration;
            this.Kind = kind;
        }
    }
}