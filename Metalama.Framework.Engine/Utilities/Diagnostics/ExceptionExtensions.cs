// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
{
    internal static class ExceptionExtensions
    {
        public static string ToDiagnosticString( this Exception ex )
            => ex.InnerException == null ? $"{ex.GetType()}: {ex.Message}" : $"{ex.GetType()}: {ex.Message} -> {ex.InnerException.ToDiagnosticString()}";
    }
}