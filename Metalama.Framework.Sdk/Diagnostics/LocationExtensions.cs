// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics;

public static class LocationExtensions
{
    public static IDiagnosticLocation ToDiagnosticLocation( this Location? location ) => new LocationWrapper( location );
}