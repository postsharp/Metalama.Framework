// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static class DiagnosticDescriptorExtensions
    {
        public static DiagnosticDescriptor ToRoslynDescriptor( this IDiagnosticDefinition definition )
            => new( definition.Id, definition.Title, definition.MessageFormat, definition.Category, definition.Severity.ToRoslynSeverity(), true );

        /// <summary>
        /// Creates an <see cref="InvalidUserCodeException"/> instance based on the current descriptor and given arguments.
        /// The diagnostic location is taken from <see cref="DiagnosticContext"/>. This method must be called in user-called code
        /// in case of precondition failure (i.e. when the responsibility of the error lays on the user).
        /// </summary>
        public static Exception CreateException<T>( this DiagnosticDefinition<T> definition, T arguments )
            where T : notnull
            => new InvalidUserCodeException( definition.CreateDiagnostic( DiagnosticContext.CurrentLocation?.GetLocation(), arguments ) );

        // Coverage: ignore (trivial)
        public static Exception CreateException<T>( this DiagnosticDefinition<T> definition, Location? location, T arguments )
            where T : notnull
            => new InvalidUserCodeException( definition.CreateDiagnostic( location ?? DiagnosticContext.CurrentLocation?.GetLocation(), arguments ) );

        // Coverage: ignore (trivial)
        public static Exception CreateException( this DiagnosticDefinition definition, params object[] arguments )
            => new InvalidUserCodeException( definition.CreateDiagnostic( DiagnosticContext.CurrentLocation?.GetLocation(), arguments ) );

        // Coverage: ignore (trivial)
        public static Exception CreateException( this DiagnosticDefinition definition, Location? location, params object[] arguments )
            => new InvalidUserCodeException( definition.CreateDiagnostic( location ?? DiagnosticContext.CurrentLocation?.GetLocation(), arguments ) );

        /// <summary>
        /// Instantiates a <see cref="Diagnostic"/> based on the current descriptor and given arguments.
        /// </summary>
        public static Diagnostic CreateDiagnostic<T>(
            this DiagnosticDefinition<T> definition,
            Location? location,
            T arguments,
            IEnumerable<Location>? additionalLocations = null )
            where T : notnull
        {
            object[] argumentArray;

            if ( typeof(T).Name.StartsWith( nameof(ValueTuple), StringComparison.OrdinalIgnoreCase ) )
            {
                argumentArray = ValueTupleAdapter.ToArray( arguments );
            }
            else
            {
                argumentArray = new object[] { arguments };
            }

            return definition.CreateDiagnosticImpl( location, argumentArray, additionalLocations );
        }

        public static Diagnostic CreateDiagnostic(
            this DiagnosticDefinition definition,
            Location? location,
            object[]? arguments = null,
            IEnumerable<Location>? additionalLocations = null )
            => definition.CreateDiagnosticImpl( location, arguments, additionalLocations );

        private static Diagnostic CreateDiagnosticImpl(
            this IDiagnosticDefinition definition,
            Location? location,
            object[]? arguments,
            IEnumerable<Location>? additionalLocations )
            => Diagnostic.Create(
                definition.Id,
                definition.Category,
                new NonLocalizedString( definition.MessageFormat, arguments ),
                definition.Severity.ToRoslynSeverity(),
                definition.Severity.ToRoslynSeverity(),
                true,
                definition.Severity == Severity.Error ? 0 : 1,
                location: location,
                additionalLocations: additionalLocations );
    }
}