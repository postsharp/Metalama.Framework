// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Enumerates the scenarios in which an aspect or a template can be executed.
    /// </summary>
    [CompileTimeOnly]
    public enum AspectExecutionScenario
    {
        /// <summary>
        /// Compile time.
        /// </summary>
        CompileTime,

        /// <summary>
        /// Design time, automatically.
        /// </summary>
        DesignTime,

        /// <summary>
        /// On demand at design time, when applying an aspect to source code.
        /// </summary>
        ApplyToSourceCode
    }
}