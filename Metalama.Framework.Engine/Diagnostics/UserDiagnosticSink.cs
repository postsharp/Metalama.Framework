// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IDiagnosticSink"/> interface
    /// and maps user-level diagnostics into Roslyn <see cref="Diagnostic"/>.
    /// </summary>
    public class UserDiagnosticSink : IDiagnosticSink, IDiagnosticAdder
    {
        private readonly DiagnosticManifest? _diagnosticManifest;
        private readonly CodeFixFilter _codeFixFilter;
        private ImmutableArray<Diagnostic>.Builder? _diagnostics;
        private ImmutableArray<ScopedSuppression>.Builder? _suppressions;
        private ImmutableArray<CodeFixInstance>.Builder? _codeFixes;

        public bool IsEmpty
        {
            get
            {
                if ( this._diagnostics is { Count: > 0 } )
                {
                    return false;
                }

                if ( this._suppressions is { Count: > 0 } )
                {
                    return false;
                }

                if ( this._codeFixes is { Count: > 0 } )
                {
                    return false;
                }

                return true;
            }
        }

        public UserDiagnosticSink( CompileTimeProject? compileTimeProject ) : this( compileTimeProject, null ) { }

        internal UserDiagnosticSink( CompileTimeProject? compileTimeProject, CodeFixFilter? codeFixFilter )
        {
            this._diagnosticManifest = compileTimeProject?.ClosureDiagnosticManifest;
            this._codeFixFilter = codeFixFilter ?? (( _, _ ) => false);
        }

        // This overload is used for tests only.
        internal UserDiagnosticSink( CodeFixFilter? codeFixFilter = null )
        {
            this._codeFixFilter = codeFixFilter ?? (( _, _ ) => false);
        }

        public void Reset()
        {
            this._diagnostics?.Clear();
            this._suppressions?.Clear();
            this._codeFixes?.Clear();
        }

        public int ErrorCount { get; private set; }
        public int Revision { get; private set; }

        public void Report( Diagnostic diagnostic )
        {
            this._diagnostics ??= ImmutableArray.CreateBuilder<Diagnostic>();
            this._diagnostics.Add( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                this.ErrorCount++;
            }

            this.Revision++;
        }

        /// <summary>
        /// Returns a string containing all code fix titles and captures the code fixes if we should.  
        /// </summary>
        private CodeFixTitles ProcessCodeFix( IDiagnosticDefinition diagnosticDefinition, Location? location, ImmutableArray<CodeFix> codeFixes )
        {
            if ( !codeFixes.IsDefaultOrEmpty )
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

            this.Revision++;
        }

        public void Suppress( IEnumerable<ScopedSuppression> suppressions )
        {
            foreach ( var suppression in suppressions )
            {
                this.Suppress( suppression );
            }
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

        public void Report( IDiagnostic diagnostic, IDiagnosticLocation? location )
        {
            this.ValidateUserReport( diagnostic.Definition );

            var resolvedLocation = GetLocation( location );
            var codeFixTitles = this.ProcessCodeFix( diagnostic.Definition, resolvedLocation, diagnostic.CodeFixes );

            this.Report( diagnostic.Definition.CreateRoslynDiagnostic( resolvedLocation, diagnostic.Arguments, codeFixes: codeFixTitles ) );
        }

        public void Suppress( SuppressionDefinition suppression, IDeclaration scope )
        {
            this.ValidateUserSuppression( suppression );
            this.Suppress( new ScopedSuppression( suppression, scope ) );
        }

        public void Suggest( CodeFix codeFix, IDiagnosticLocation location )
        {
            var definition = GeneralDiagnosticDescriptors.SuggestedCodeFix;
            var resolvedLocation = GetLocation( location );
            var codeFixTitles = this.ProcessCodeFix( definition, resolvedLocation, ImmutableArray.Create( codeFix ) );

            this.Report( definition.CreateRoslynDiagnostic( resolvedLocation, codeFixTitles.Value!, codeFixes: codeFixTitles ) );
        }

        public void AddCodeFixes( IEnumerable<CodeFixInstance> codeFixes )
        {
            foreach ( var codeFix in codeFixes )
            {
                this._codeFixes ??= ImmutableArray.CreateBuilder<CodeFixInstance>();
                this._codeFixes.Add( codeFix );

                this.Revision++;
            }
        }
    }
}