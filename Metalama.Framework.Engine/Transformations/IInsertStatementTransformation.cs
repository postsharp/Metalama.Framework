// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a code transformation that insert statements into the target member.
    /// </summary>
    internal interface IInsertStatementTransformation : ITransformation
    {
        /// <summary>
        /// Gets a target member into which the statement should be inserted.
        /// </summary>
        IMember TargetMember { get; }

        /// <summary>
        /// Provides an list of inserted statements.
        /// </summary>
        /// <param name="context">Context for providing inserted statements.</param>
        /// <returns>A list of Inserted statements or empty list if an error occured.</returns>
        IReadOnlyList<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context );
    }
}