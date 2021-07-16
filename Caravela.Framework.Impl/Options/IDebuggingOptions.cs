// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Options
{
    /// <summary>
    /// Exposes options allowing to attach a debugger to different compile-time or design-time processes.
    /// </summary>
    public interface IDebuggingOptions : IService
    {
        /// <summary>
        /// Gets a value indicating whether the debugger should be attached to the compiler process.
        /// </summary>
        bool DebugCompilerProcess { get; }

        /// <summary>
        /// Gets a value indicating whether the debugger should be attached to the interactive analysis process (a child process of devenv).
        /// </summary>
        bool DebugAnalyzerProcess { get; }

        /// <summary>
        /// Gets a value indicating whether the debugger should be attached to the interactive analysis process.
        /// </summary>
        public bool DebugIdeProcess { get; }
    }
}