// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Stores the default location for diagnostics. This class is used so that methods called by user code can throw exceptions
    /// with location information although user code does not pass this location. Any code calling user code should set
    /// the current context using the <see cref="WithDefaultLocation"/> method.
    /// </summary>
    public static partial class DiagnosticContext
    {
        private static readonly AsyncLocal<IDiagnosticLocation?> _current = new();

        /// <summary>
        /// Gets the location on which diagnostics should be created when the location is not otherwise available.
        /// </summary>
        public static IDiagnosticLocation? CurrentLocation => _current.Value;

        public static IDisposable WithDefaultLocation( IDiagnosticLocation? location )
        {
            var cookie = new Cookie( _current.Value );
            _current.Value = location;
            return cookie;
        }
    }
}