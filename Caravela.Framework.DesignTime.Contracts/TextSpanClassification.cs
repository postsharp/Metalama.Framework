// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.

    /// <summary>
    /// An enumeration of classifications of <see cref="TextSpanClassification"/>, which typically
    /// map to different colors in the view layer.
    /// </summary>
    public enum TextSpanClassification
    {
        // NOTE: Order of declaration (or at last enum value) matters. The higher value overwrites the lower.
        // NOTE: Renaming these items will break the string-based tests.

        /// <summary>
        /// No category.
        /// </summary>
        Default,

        /// <summary>
        /// Run-time code.
        /// </summary>
        RunTime,

        /// <summary>
        /// Compile-time code.
        /// </summary>
        CompileTime,

        /// <summary>
        /// Dynamic member.
        /// </summary>
        Dynamic,

        /// <summary>
        /// Compile-time variable.
        /// </summary>
        CompileTimeVariable,

        /// <summary>
        /// Keyword-like member.
        /// </summary>
        TemplateKeyword,

        /// <summary>
        /// A text span has several categories (not implemented).
        /// </summary>
        Conflict
    }
}