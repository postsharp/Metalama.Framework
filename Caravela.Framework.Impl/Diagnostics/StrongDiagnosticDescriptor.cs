// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// A strongly typed <see cref="DiagnosticDescriptor"/>.
    /// </summary>
    /// <typeparam name="T">Type of arguments: a single type if there is a single argument, or a named tuple type.</typeparam>
    internal partial class StrongDiagnosticDescriptor<T> : IStrongDiagnosticDescriptor
    {
        public StrongDiagnosticDescriptor( string id, string title, string messageFormat, string category, DiagnosticSeverity severity )
        {
            this.Severity = severity;
            this.Id = id;
            this.MessageFormat = messageFormat;
            this.Title = title;
            this.Category = category;
        }

        public DiagnosticSeverity Severity { get; }

        public string Id { get; }

        public string MessageFormat { get; }

        public string Category { get; }

        public string Title { get; }

        /// <summary>
        /// Creates an <see cref="InvalidUserCodeException"/> instance based on the current descriptor and given arguments.
        /// The diagnostic location is taken from <see cref="DiagnosticContext"/>. This method must be called in user-called code
        /// in case of precondition failure (i.e. when the responsibility of the error lays on the user).
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Exception CreateException( T arguments )
            => new InvalidUserCodeException( this.CreateDiagnostic( DiagnosticContext.CurrentLocation?.GetLocation(), arguments ) );

        /// <summary>
        /// Instantiates a <see cref="Diagnostic"/> based on the current descriptor and given arguments.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="arguments"></param>
        /// <param name="additionalLocations"></param>
        /// <returns></returns>
        public Diagnostic CreateDiagnostic( Location? location, T arguments, IEnumerable<Location>? additionalLocations = null )
        {
            if ( arguments == null )
            {
                throw new ArgumentNullException( nameof( arguments ) );
            }

            object[] argumentArray;

            if ( typeof( T ).Name.StartsWith( nameof( ValueTuple ), StringComparison.OrdinalIgnoreCase ) )
            {
                argumentArray = ValueTupleAdapter.ToArray( arguments );
            }
            else
            {
                argumentArray = new object[] { arguments };
            }

            return Diagnostic.Create(
                this.Id,
                this.Category,
                new NonLocalizedString( this.MessageFormat, argumentArray ),
                this.Severity,
                this.Severity,
                true,
                this.Severity == DiagnosticSeverity.Error ? 0 : 1,
                location: location,
                additionalLocations: additionalLocations );
        }
    }
}