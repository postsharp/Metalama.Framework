// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics;

#pragma warning disable CA1305

public static class DiagnosticExtensions
{
    public static string GetLocalizedMessage( this Diagnostic diagnostic ) => diagnostic.GetMessage( MetalamaStringFormatter.Instance );
}