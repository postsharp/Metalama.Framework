// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a single code transformation.
    /// </summary>
    internal interface IInsertStatementTransformation : IMemberLevelTransformation
    {
        /// <summary>
        /// Evaluates the target syntax node and transforms the state.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Inserted statement or <c>null</c> if an error has occured.</returns>
        IEnumerable<InsertedStatement> GetInsertedStatements( InsertStatementTransformationContext context );

        // TODO: There is currently no notion of order of inserted statements, they are just inserted in transformation order.
        //       This is fine for initialization, which is currently the only use case.
    }
}