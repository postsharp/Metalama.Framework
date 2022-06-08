// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Diagnostics
{
    public static class DiagnosticDescriptorExtensions
    {
        public static DiagnosticDescriptor ToRoslynDescriptor( this IDiagnosticDefinition definition )
            => new( definition.Id, definition.Title, definition.MessageFormat, definition.Category, definition.Severity.ToRoslynSeverity(), true );

        /// <summary>
        /// Creates an <see cref="DiagnosticException"/> instance based on the current descriptor and given arguments.
        /// The diagnostic location will be resolved from the call stack.
        /// </summary>
        public static Exception CreateException<T>( this DiagnosticDefinition<T> definition, T arguments )
            where T : notnull
            => new DiagnosticException( definition.CreateRoslynDiagnostic( null, arguments ) );

        // Coverage: ignore (trivial)
        public static Exception CreateException<T>( this DiagnosticDefinition<T> definition, Location location, T arguments )
            where T : notnull
            => new DiagnosticException( definition.CreateRoslynDiagnostic( location, arguments ) );

        /// <summary>
        /// Instantiates a <see cref="Diagnostic"/> based on the current descriptor and given arguments.
        /// </summary>
        public static Diagnostic CreateRoslynDiagnostic<T>(
            this DiagnosticDefinition<T> definition,
            Location? location,
            T arguments,
            IEnumerable<Location>? additionalLocations = null,
            CodeFixTitles codeFixes = default,
            ImmutableDictionary<string, string?>? properties = null )
            where T : notnull
        {
            var argumentArray = ConvertDiagnosticArguments( arguments );

            return definition.CreateRoslynDiagnosticImpl( location, argumentArray, additionalLocations, codeFixes, properties );
        }

        internal static Diagnostic CreateRoslynDiagnosticImpl(
            this IDiagnosticDefinition definition,
            Location? location,
            object? arguments,
            IEnumerable<Location>? additionalLocations = null,
            CodeFixTitles codeFixes = default,
            ImmutableDictionary<string, string?>? properties = null )
        {
            var argumentArray = ConvertDiagnosticArguments( arguments );

            return definition.CreateRoslynDiagnosticImpl( location, argumentArray, additionalLocations, codeFixes, properties );
        }

        private static object?[] ConvertDiagnosticArguments( object? arguments )
        {
            object?[] argumentArray;

            if ( arguments == null )
            {
                return Array.Empty<object?>();
            }

            if ( arguments.GetType().Name.StartsWith( nameof(ValueTuple), StringComparison.OrdinalIgnoreCase ) )
            {
                argumentArray = ValueTupleAdapter.ToArray( arguments );
            }
            else if ( arguments.GetType().IsArray )
            {
                argumentArray = (object[]) arguments;
            }
            else
            {
                argumentArray = new[] { arguments };
            }

            return argumentArray;
        }

        private static Diagnostic CreateRoslynDiagnosticImpl(
            this IDiagnosticDefinition definition,
            Location? location,
            object?[] arguments,
            IEnumerable<Location>? additionalLocations,
            CodeFixTitles codeFixes = default,
            ImmutableDictionary<string, string?>? properties = null )
        {
            var propertiesWithCodeFixes = properties;

            if ( codeFixes.Value != null )
            {
                propertiesWithCodeFixes ??= ImmutableDictionary.Create<string, string?>();
                propertiesWithCodeFixes = propertiesWithCodeFixes.Add( CodeFixTitles.DiagnosticPropertyKey, codeFixes.Value );
            }

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
                properties: propertiesWithCodeFixes );
        }
    }
}