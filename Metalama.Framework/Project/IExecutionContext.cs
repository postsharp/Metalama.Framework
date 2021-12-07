// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Project
{
    /// <summary>
    /// Represents the execution context of Metalama. Exposed by the <see cref="MetalamaExecutionContext.Current"/> property of <see cref="MetalamaExecutionContext"/>.
    /// </summary>
    public interface IExecutionContext
    {
        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the <see cref="IFormatProvider"/>, used to format formattable strings. This format provider will properly format elements of the code model
        /// and other similar objects.
        /// </summary>
        IFormatProvider FormatProvider { get; }

        /// <summary>
        /// Gets the current compilation.
        /// </summary>
        ICompilation Compilation { get; }

        /// <summary>
        /// Gets information about why the current code is executed and what are the abilities of the current execution context.
        /// </summary>
        IExecutionScenario ExecutionScenario { get; }
    }
}