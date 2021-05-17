// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Exposes the list of diagnostics and suppressions defined in the project.
    /// </summary>
    public sealed class DiagnosticManifest
    {
        private readonly ImmutableHashSet<string> _definedDiagnostics;
        private readonly ImmutableHashSet<string> _definedSuppressions;

        public ImmutableArray<IDiagnosticDefinition> DiagnosticDescriptions { get; }

        public ImmutableArray<SuppressionDefinition> SuppressionDescriptions { get; }

        public DiagnosticManifest( ImmutableArray<IDiagnosticDefinition> diagnosticDescriptions, ImmutableArray<SuppressionDefinition> suppressionDescriptions )
        {
            this.DiagnosticDescriptions = diagnosticDescriptions;
            this.SuppressionDescriptions = suppressionDescriptions;
            this._definedDiagnostics = diagnosticDescriptions.Select( d => d.Id ).ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );
            this._definedSuppressions = suppressionDescriptions.Select( d => d.SuppressedDiagnosticId ).ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );
        }

        public DiagnosticManifest( IReadOnlyCollection<DiagnosticManifest> items )
        {
            this.DiagnosticDescriptions = items.SelectMany( i => i.DiagnosticDescriptions ).ToImmutableArray();
            this.SuppressionDescriptions = items.SelectMany( i => i.SuppressionDescriptions ).ToImmutableArray();
            this._definedDiagnostics = this.DiagnosticDescriptions.Select( d => d.Id ).ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            this._definedSuppressions = this.SuppressionDescriptions.Select( d => d.SuppressedDiagnosticId )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );
        }

        public bool DefinesDiagnostic( string id ) => this._definedDiagnostics.Contains( id );

        public bool DefinesSuppression( string id ) => this._definedSuppressions.Contains( id );
    }
}