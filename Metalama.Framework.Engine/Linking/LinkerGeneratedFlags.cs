// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Linking
{
    [Flags]
    internal enum LinkerGeneratedFlags
    {
        None = 0,

        /// <summary>
        /// Block that was added by the linker but can be flattened.
        /// </summary>
        FlattenableBlock = 1,

        /// <summary>
        /// Labeled empty statement added by the linker and can be attached to the next statement.
        /// </summary>
        EmptyLabeledStatement = 2,

        /// <summary>
        /// Empty statement that was added by the linker to carry trivias, which should be attached to surrounding statements.
        /// </summary>
        EmptyTriviaStatement = 4,

        /// <summary>
        /// Null literal expression that replaced the original aspect reference. It's parent expression should be removed.
        /// </summary>
        NullAspectReferenceExpression = 8,
    }
}