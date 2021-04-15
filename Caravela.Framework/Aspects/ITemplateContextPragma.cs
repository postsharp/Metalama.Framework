// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes methods that add trivias to the transformed. code.
    /// </summary>
    [CompileTimeOnly]
    public interface ITemplateContextPragma
    {
        /// <summary>
        /// Injects a comment to the target code.
        /// </summary>
        /// <param name="lines">A list of comment lines, without the <c>//</c> prefix. Null strings are processed as blank ones and will inject a blank comment line.</param>
        /// <remarks>
        /// This method is not able to add a comment to an empty block. The block must contain at least one statement.
        /// </remarks>
        [Pragma]
        void Comment( params string?[] lines );
    }
}