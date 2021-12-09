// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;

namespace Metalama.Framework.Project
{
    /// <summary>
    /// Exposes the current execution context of Metalama.
    /// </summary>
    public static class MetalamaExecutionContext
    {
        private static readonly AsyncLocal<IExecutionContext?> _current = new();

        /// <summary>
        /// Gets the current execution context, or throws an exception if there no execution context.
        /// </summary>
        public static IExecutionContext Current => _current.Value ?? throw new InvalidOperationException();

        /// <summary>
        /// Gets or sets the current execution context, or <c>null</c> if there is no execution context.
        /// </summary>
        internal static IExecutionContext? CurrentOrNull
        {
            get => _current.Value;
            set => _current.Value = value;
        }
    }
}