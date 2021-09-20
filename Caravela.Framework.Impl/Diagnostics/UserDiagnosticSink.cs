// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IDiagnosticSink"/> interface
    /// and maps user-level diagnostics into Roslyn <see cref="Diagnostic"/>.
    /// </summary>
    internal partial class UserDiagnosticSink : IDiagnosticSink, IDiagnosticAdder
    {
        private readonly DiagnosticManifest? _diagnosticManifest;
        private ImmutableArray<Diagnostic>.Builder? _diagnostics;
        private ImmutableArray<ScopedSuppression>.Builder? _suppressions;

        public IDeclaration? DefaultScope { get; private set; }

        internal UserDiagnosticSink( CompileTimeProject? compileTimeProject, IDeclaration? defaultScope = null )
        {
            this._diagnosticManifest = compileTimeProject?.ClosureDiagnosticManifest;
            this.DefaultScope = defaultScope;
        }

        // This overload is used for tests only.
        internal UserDiagnosticSink( IDeclaration? defaultScope = null )
        {
            this.DefaultScope = defaultScope;
        }

        public int ErrorCount { get; private set; }

        public void Report( Diagnostic diagnostic )
        {
            this._diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
            this._diagnostics.Add( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.ErrorCount++;
            }
        }

        public void Suppress( ScopedSuppression suppression )
        {
            this._suppressions ??= ImmutableArray.CreateBuilder<ScopedSuppression>();
            this._suppressions.Add( suppression );
        }

        public void Suppress( IEnumerable<ScopedSuppression> suppressions )
        {
            foreach ( var suppression in suppressions )
            {
                this.Suppress( suppression );
            }
        }

        public IDisposable WithDefaultScope( IDeclaration scope )
        {
            var oldScope = this.DefaultScope;
            this.DefaultScope = scope;

            return new RestoreLocationCookie( this, oldScope );
        }

        public ImmutableUserDiagnosticList ToImmutable()
            => new(
                this._diagnostics?.ToImmutable() ?? ImmutableArray<Diagnostic>.Empty,
                this._suppressions?.ToImmutable() ?? ImmutableArray<ScopedSuppression>.Empty );

        public override string ToString() => $"Diagnostics={this._diagnostics?.Count ?? 0}, Suppressions={this._suppressions?.Count ?? 0}";

        private Location? GetLocation( IDiagnosticLocation? location )
            => ((DiagnosticLocation?) location)?.Location ?? this.DefaultScope?.GetDiagnosticLocation();

        private void ValidateUserReport( IDiagnosticDefinition definition )
        {
            if ( this._diagnosticManifest != null && !this._diagnosticManifest.DefinesDiagnostic( definition.Id ) )
            {
                throw new InvalidOperationException(
                    $"The aspect cannot report the diagnostic {definition.Id} because the DiagnosticDefinition is not declared as a static field or property of the aspect class." );
            }
        }

        private void ValidateUserSuppression( SuppressionDefinition definition )
        {
            if ( this._diagnosticManifest != null && !this._diagnosticManifest.DefinesSuppression( definition.SuppressedDiagnosticId ) )
            {
                throw new InvalidOperationException(
                    $"The aspect cannot suppress the diagnostic {definition.SuppressedDiagnosticId} because the SuppressionDefinition is not declared as a static field or property of the aspect class." );
            }
        }

        public void Report( IDiagnosticLocation? location, DiagnosticDefinition definition, params object[] args )
        {
            this.ValidateUserReport( definition );
            this.Report( definition.CreateDiagnostic( this.GetLocation( location ) ) );
        }

        public void Report<T>( IDiagnosticLocation? location, DiagnosticDefinition<T> definition, T arguments )
            where T : notnull
        {
            this.ValidateUserReport( definition );
            this.Report( definition.CreateDiagnostic( this.GetLocation( location ), arguments ) );
        }

        public void Suppress( IDeclaration? scope, SuppressionDefinition definition )
        {
            this.ValidateUserSuppression( definition );

            if ( this.DefaultScope != null )
            {
                this.Suppress( new ScopedSuppression( definition, scope ?? this.DefaultScope ) );
            }
        }
    }
}