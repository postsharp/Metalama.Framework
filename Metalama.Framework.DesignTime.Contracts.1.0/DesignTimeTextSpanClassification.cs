// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.DesignTime.Contracts
{
    // The type identifier cannot be modified even during refactoring.

    /// <summary>
    /// An enumeration of classifications of text spans, which typically
    /// map to different colors in the view layer.
    /// </summary>
    public enum DesignTimeTextSpanClassification
    {
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
        Conflict,

        /// <summary>
        /// An excluded region, which should not be written to the output file (used for HTML generation).
        /// </summary>
        Excluded,

        /// <summary>
        /// Used to classify the output code and marks the code generated by the aspect or by the framework.
        /// </summary>
        GeneratedCode,

        /// <summary>
        /// Used to classify the output code and marks the source code.
        /// </summary>
        SourceCode
    }
}