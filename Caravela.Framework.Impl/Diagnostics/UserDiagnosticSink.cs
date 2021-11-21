// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.CodeFixes;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.CodeFixes;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IDiagnosticSink"/> interface
    /// and maps user-level diagnostics into Roslyn <see cref="Diagnostic"/>.
    /// </summary>
    internal partial class UserDiagnosticSink : IDiagnosticSink, IDiagnosticAdder
    {
        private readonly DiagnosticManifest? _diagnosticManifest;
        private readonly CodeFixFilter _codeFixFilter;
        private ImmutableArray<Diagnostic>.Builder? _diagnostics;
        private ImmutableArray<ScopedSuppression>.Builder? _suppressions;
        private ImmutableArray<CodeFixInstance>.Builder? _codeFixes;

        public IDeclaration? DefaultScope { get; private set; }

        internal UserDiagnosticSink( CompileTimeProject? compileTimeProject, CodeFixFilter? codeFixFilter, IDeclaration? defaultScope = null )
        {
            this._diagnosticManifest = compileTimeProject?.ClosureDiagnosticManifest;
            this._codeFixFilter = codeFixFilter ?? (( _, _ ) => false);
            this.DefaultScope = defaultScope;
        }

        // This overload is used for tests only.
        internal UserDiagnosticSink( IDeclaration? defaultScope = null, CodeFixFilter? codeFixFilter = null )
        {
            this.DefaultScope = defaultScope;
            this._codeFixFilter = codeFixFilter ?? (( _, _ ) => false);
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

        /// <summary>
        /// Returns a string containing all code fix titles and captures the code fixes if we should.  
        /// </summary>
        private CodeFixTitles ProcessCodeFix( IDiagnosticDefinition diagnosticDefinition, Location? location, IEnumerable<CodeFix>? codeFixes )
        {
            if ( codeFixes != null )
            {
                // This code implements an optimization to allow allocating a StringBuilder if there is a single code fix. 
                string? firstTitle = null;
                StringBuilder? stringBuilder = null;

                // Store the code fixes if we should.
                foreach ( var codeFix in codeFixes )
                {
                    if ( location != null && this._codeFixFilter( diagnosticDefinition, location ) )
                    {
                        this._codeFixes ??= ImmutableArray.CreateBuilder<CodeFixInstance>();
                        this._codeFixes.Add( new CodeFixInstance( diagnosticDefinition.Id, location, codeFix ) );
                    }

                    if ( firstTitle == null )
                    {
                        // This gets executed for the first code fix.
                        firstTitle = codeFix.Title;
                    }
                    else
                    {
                        if ( stringBuilder == null )
                        {
                            // This gets executed for the second code fix.
                            stringBuilder = new StringBuilder();
                            stringBuilder.Append( firstTitle );
                        }

                        // This gets executed for all code fixes but the first one.
                        stringBuilder.Append( CodeFixTitles.Separator );
                        stringBuilder.Append( codeFix.Title );
                    }
                }

                return new CodeFixTitles( stringBuilder?.ToString() ?? firstTitle );
            }
            else
            {
                return default;
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
            => new( this._diagnostics?.ToImmutable(), this._suppressions?.ToImmutable(), this._codeFixes?.ToImmutable() );

        public override string ToString()
            => $"Diagnostics={this._diagnostics?.Count ?? 0}, Suppressions={this._suppressions?.Count ?? 0}, CodeFixes={this._codeFixes?.Count ?? 0}";

        private static Location? GetLocation( IDiagnosticLocation? location ) => ((IDiagnosticLocationImpl?) location)?.DiagnosticLocation;

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

        public void Report<T>(
            IDiagnosticLocation? location,
            DiagnosticDefinition<T> definition,
            T arguments,
            IEnumerable<CodeFix>? codeFixes = null )
            where T : notnull
        {
            this.ValidateUserReport( definition );

            var resolvedLocation = GetLocation( location );
            var codeFixTitles = this.ProcessCodeFix( definition, resolvedLocation, codeFixes );

            this.Report( definition.CreateDiagnostic( resolvedLocation, arguments, codeFixes: codeFixTitles ) );
        }

        public void Suppress( IDeclaration? scope, SuppressionDefinition definition )
        {
            this.ValidateUserSuppression( definition );

            if ( this.DefaultScope != null )
            {
                this.Suppress( new ScopedSuppression( definition, scope ?? this.DefaultScope ) );
            }
        }

        public void Suggest( IDiagnosticLocation? location, IEnumerable<CodeFix>? codeFixes )
        {
            var definition = GeneralDiagnosticDescriptors.SuggestedCodeFix;
            var resolvedLocation = GetLocation( location );
            var codeFixTitles = this.ProcessCodeFix( definition, resolvedLocation, codeFixes );

            this.Report( definition.CreateDiagnostic( resolvedLocation, default, codeFixes: codeFixTitles ) );
        }

        public void AddCodeFixes( IEnumerable<CodeFixInstance> codeFixes )
        {
            foreach ( var codeFix in codeFixes )
            {
                this._codeFixes ??= ImmutableArray.CreateBuilder<CodeFixInstance>();
                this._codeFixes.Add( codeFix );
            }
        }
    }
}