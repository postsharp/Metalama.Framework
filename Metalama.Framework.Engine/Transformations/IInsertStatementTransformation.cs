// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a single code transformation.
    /// </summary>
    internal interface IInsertStatementTransformation : INonObservableTransformation, ISyntaxTreeTransformation
    {
        /// <summary>
        /// Gets a context of this code transformation. If there are transformation marks on the same syntax node, those coming from member-context
        /// transformations precede type-context transformation. Member-context transformations do not have defined order between them.
        /// </summary>
        IMemberOrNamedType ContextDeclaration { get; }

        /// <summary>
        /// Gets a target method base of this code transformation.
        /// </summary>
        IMethodBase TargetDeclaration { get; }

        /// <summary>
        /// Evaluates the target syntax node and transforms the state.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Inserted statement or <c>null</c> if an error has occured.</returns>
        InsertedStatement? GetInsertedStatement( InsertStatementTransformationContext context );

        // TODO: There is currently no notion of order of inserted statements, they are just inserted in transformation order.
        //       This is fine for initialization, which is currently the only use case.
    }
}