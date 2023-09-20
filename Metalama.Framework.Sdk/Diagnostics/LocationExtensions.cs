using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics;

public static class LocationExtensions
{
    public static IDiagnosticLocation ToDiagnosticLocation( this Location? location ) => new LocationWrapper( location );
}