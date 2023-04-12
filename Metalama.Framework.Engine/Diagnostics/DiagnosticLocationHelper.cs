// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Helper methods to work with diagnostic locations.
    /// </summary>
    public static class DiagnosticLocationHelper
    {
        internal static Location? GetLocation( this IDiagnosticLocation location ) => ((IDiagnosticLocationImpl) location).DiagnosticLocation;

        /// <summary>
        /// Gets the <see cref="Location"/> suitable to report a <see cref="Diagnostic"/> on
        /// a given <see cref="ISymbol"/> (typically the identifier).
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static Location? GetLocationForDiagnostic( this ISymbol symbol ) => symbol.GetLocationForDiagnosticImpl( 0 );

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static Location? GetLocationForDiagnosticImpl( this ISymbol symbol, int depth )
        {
            if ( depth > 8 )
            {
                throw new AssertionFailedException( $"Infinite recursion in getting the location for symbol '{symbol}'." );
            }

            var bestDeclaration = symbol.GetPrimarySyntaxReference();

            if ( bestDeclaration == null )
            {
                if ( symbol.ContainingSymbol != null && symbol.ContainingSymbol.Kind != SymbolKind.Namespace )
                {
                    // Implicit symbols do not have a syntax. In this case, we go to the parent declaration.
                    return symbol.ContainingSymbol?.GetLocationForDiagnosticImpl( depth + 1 );
                }
                else
                {
                    // We don't walk lower than namespaces. This makes sense and works around a bug in configuration of linker fakes.
                    return null;
                }
            }
            else
            {
                return bestDeclaration.GetSyntax().GetLocationForDiagnostic();
            }
        }

        /// <summary>
        /// Gets the <see cref="Location"/> suitable to report a <see cref="Diagnostic"/> on
        /// a given <see cref="AttributeData"/>.
        /// </summary>
        internal static Location? GetLocationForDiagnostic( this AttributeData attribute )
            => attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation();
    }
}