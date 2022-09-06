﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// An exception thrown by Metalama, embedding a <see cref="Diagnostic"/>, thrown in a situation where
    /// the responsibility can be put on the user. This exception type is typically not observed out of Metalama code,
    ///  and should be handled properly.
    /// </summary>
    public sealed class DiagnosticException : Exception
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        internal DiagnosticException( string message, ImmutableArray<Diagnostic> diagnostics ) : base( GetMessage( message, diagnostics ) )
        {
            this.Diagnostics = diagnostics;
        }

        internal DiagnosticException( Diagnostic diagnostic )
            : base( diagnostic.ToString() )
        {
            this.Diagnostics = ImmutableArray.Create( diagnostic );
        }

        private static string GetMessage( string message, IReadOnlyList<Diagnostic> diagnostics )
            => message + Environment.NewLine + string.Join( Environment.NewLine, diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ) );
    }
}