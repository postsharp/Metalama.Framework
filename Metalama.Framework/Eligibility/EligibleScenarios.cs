// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Eligibility
{
    /// <summary>
    /// Enumeration of scenarios in which an aspect can be used.
    /// </summary>
    [CompileTimeOnly]
    [Flags]
    public enum EligibleScenarios
    {
        /// <summary>
        /// Means that the aspect can neither be applied to the target declaration, nor to declarations derived from the the
        /// target declaration, nor as a live template.
        /// </summary>
        None = 0,

        /// <summary>
        /// Means that the aspect can be applied to declarations that are derived from the target declaration, but not on
        /// the target declaration itself.
        /// </summary>
        Inheritance = 1,

        /// <summary>
        /// Means that the aspect can be applied to the target declaration.
        /// </summary>
        Aspect = 2,

        /// <summary>
        /// Means that the aspect can be used as a live template on the target declaration.
        /// </summary>
        LiveTemplate = 4,

        /// <summary>
        /// Means that the aspect can be used in any scenario.
        /// </summary>
        All = Inheritance | Aspect | LiveTemplate
    }
}