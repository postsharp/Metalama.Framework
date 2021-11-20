// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static class DiagnosticDescriptorExtensions
    {
        public const string DiagnosticPropertyKey = "Caravela.HasCodeFix";

        private static readonly ImmutableDictionary<string, string?> _propertiesOfDiagnosticWithCodeFix =
            ImmutableDictionary.Create<string, string?>().Add( DiagnosticPropertyKey, "true" );

        public static DiagnosticDescriptor ToRoslynDescriptor( this IDiagnosticDefinition definition )
            => new( definition.Id, definition.Title, definition.MessageFormat, definition.Category, definition.Severity.ToRoslynSeverity(), true );

        /// <summary>
        /// Creates an <see cref="DiagnosticException"/> instance based on the current descriptor and given arguments.
        /// The diagnostic location is taken from <see cref="DiagnosticContext"/>. This method must be called in user-called code
        /// in case of precondition failure (i.e. when the responsibility of the error lays on the user).
        /// </summary>
        public static Exception CreateException<T>( this DiagnosticDefinition<T> definition, T arguments )
            where T : notnull
            => new DiagnosticException( definition.CreateDiagnostic( DiagnosticContext.CurrentLocation?.GetLocation(), arguments ) );

        // Coverage: ignore (trivial)
        public static Exception CreateException<T>( this DiagnosticDefinition<T> definition, Location? location, T arguments )
            where T : notnull
            => new DiagnosticException( definition.CreateDiagnostic( location ?? DiagnosticContext.CurrentLocation?.GetLocation(), arguments ) );

        // Coverage: ignore (trivial)
        public static Exception CreateException( this DiagnosticDefinition definition )
            => new DiagnosticException( definition.CreateDiagnostic( DiagnosticContext.CurrentLocation?.GetLocation() ) );

        // Coverage: ignore (trivial)
        public static Exception CreateException( this DiagnosticDefinition definition, Location? location )
            => new DiagnosticException( definition.CreateDiagnostic( location ?? DiagnosticContext.CurrentLocation?.GetLocation() ) );

        /// <summary>
        /// Instantiates a <see cref="Diagnostic"/> based on the current descriptor and given arguments.
        /// </summary>
        public static Diagnostic CreateDiagnostic<T>(
            this DiagnosticDefinition<T> definition,
            Location? location,
            T arguments,
            IEnumerable<Location>? additionalLocations = null,
            bool hasCodeFix = false )
            where T : notnull
        {
            object[] argumentArray;

            if ( typeof(T).Name.StartsWith( nameof(ValueTuple), StringComparison.OrdinalIgnoreCase ) )
            {
                argumentArray = ValueTupleAdapter.ToArray( arguments );
            }
            else if ( arguments.GetType().IsArray )
            {
                argumentArray = (object[]) (object) arguments;
            }
            else
            {
                argumentArray = new object[] { arguments };
            }

            return definition.CreateDiagnosticImpl( location, argumentArray, additionalLocations, hasCodeFix );
        }

        public static Diagnostic CreateDiagnostic(
            this DiagnosticDefinition definition,
            Location? location,
            IEnumerable<Location>? additionalLocations = null,
            bool hasCodeFix = false )
            => definition.CreateDiagnosticImpl( location, Array.Empty<object>(), additionalLocations, hasCodeFix );

        private static Diagnostic CreateDiagnosticImpl(
            this IDiagnosticDefinition definition,
            Location? location,
            object[]? arguments,
            IEnumerable<Location>? additionalLocations,
            bool hasCodeFix = false )
        {
            return Diagnostic.Create(
                definition.Id,
                definition.Category,
                new NonLocalizedString( definition.MessageFormat, arguments ),
                definition.Severity.ToRoslynSeverity(),
                definition.Severity.ToRoslynSeverity(),
                true,
                definition.Severity == Severity.Error ? 0 : 1,
                location: location,
                additionalLocations: additionalLocations,
                properties: hasCodeFix ? _propertiesOfDiagnosticWithCodeFix : ImmutableDictionary<string, string?>.Empty );
        }
    }
}