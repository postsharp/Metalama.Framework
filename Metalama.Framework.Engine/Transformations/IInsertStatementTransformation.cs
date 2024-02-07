// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a single code transformation.
    /// </summary>
    internal interface IInsertStatementTransformation : ITransformation
    {
        /// <summary>
        /// Gets a target member into which the statement should be inserted.
        /// </summary>
        IMember TargetMember { get; }

        /// <summary>
        /// Evaluates the target syntax node and transforms the state.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Inserted statement or <c>null</c> if an error has occured.</returns>
        IEnumerable<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context );
    }
}