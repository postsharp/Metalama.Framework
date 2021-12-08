// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime
{
    /// <summary>
    /// Exposes the list of diagnostics and suppressions defined in the project.
    /// </summary>
    internal sealed class DiagnosticManifest
    {
        private readonly ImmutableHashSet<string> _definedDiagnostics;
        private readonly ImmutableHashSet<string> _definedSuppressions;

        public ImmutableArray<IDiagnosticDefinition> DiagnosticDefinitions { get; }

        public ImmutableArray<SuppressionDefinition> SuppressionDefinitions { get; }

        public DiagnosticManifest( ImmutableArray<IDiagnosticDefinition> diagnosticDescriptions, ImmutableArray<SuppressionDefinition> suppressionDescriptions )
        {
            this.DiagnosticDefinitions = diagnosticDescriptions;
            this.SuppressionDefinitions = suppressionDescriptions;
            this._definedDiagnostics = diagnosticDescriptions.Select( d => d.Id ).ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );
            this._definedSuppressions = suppressionDescriptions.Select( d => d.SuppressedDiagnosticId ).ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );
        }

        public DiagnosticManifest( IReadOnlyCollection<DiagnosticManifest> items )
        {
            this.DiagnosticDefinitions = items.SelectMany( i => i.DiagnosticDefinitions ).ToImmutableArray();
            this.SuppressionDefinitions = items.SelectMany( i => i.SuppressionDefinitions ).ToImmutableArray();
            this._definedDiagnostics = this.DiagnosticDefinitions.Select( d => d.Id ).ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );

            this._definedSuppressions = this.SuppressionDefinitions.Select( d => d.SuppressedDiagnosticId )
                .ToImmutableHashSet( StringComparer.OrdinalIgnoreCase );
        }

        public bool DefinesDiagnostic( string id ) => this._definedDiagnostics.Contains( id );

        public bool DefinesSuppression( string id ) => this._definedSuppressions.Contains( id );
    }
}