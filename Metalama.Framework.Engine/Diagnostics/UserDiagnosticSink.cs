// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.CompileTime.Manifest;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Utilities.UserCode;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Implements the user-level <see cref="IDiagnosticSink"/> interface
    /// and maps user-level diagnostics into Roslyn <see cref="Diagnostic"/>.
    /// </summary>
    public sealed class UserDiagnosticSink : IUserDiagnosticSink
    {
        private readonly DiagnosticManifest? _diagnosticManifest;
        private readonly CodeFixFilter _codeFixFilter;
        private readonly CodeFixAvailability _codeFixAvailability;
        private ConcurrentLinkedList<Diagnostic>? _diagnostics;
        private ConcurrentLinkedList<ScopedSuppression>? _suppressions;
        private ConcurrentLinkedList<CodeFixInstance>? _codeFixes;
        private int _errorCount;

        internal int ErrorCount => this._errorCount;

        internal bool IsEmpty
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

        internal UserDiagnosticSink( CompileTimeProject? compileTimeProject ) : this( compileTimeProject?.ClosureDiagnosticManifest, null ) { }

        public UserDiagnosticSink( DiagnosticManifest? diagnosticManifest ) : this( diagnosticManifest, null ) { }

        internal UserDiagnosticSink(
            CompileTimeProject? compileTimeProject,
            CodeFixFilter? codeFixFilter,
            CodeFixAvailability codeFixAvailability = CodeFixAvailability.PreviewAndApply ) : this(
            compileTimeProject?.ClosureDiagnosticManifest,
            codeFixFilter,
            codeFixAvailability ) { }

        private UserDiagnosticSink(
            DiagnosticManifest? diagnosticManifest,
            CodeFixFilter? codeFixFilter,
            CodeFixAvailability codeFixAvailability = CodeFixAvailability.PreviewAndApply )
        {
            this._diagnosticManifest = diagnosticManifest;
            this._codeFixFilter = codeFixFilter ?? (( _, _ ) => false);
            this._codeFixAvailability = codeFixAvailability;
        }

        // This overload is used for tests only.
        internal UserDiagnosticSink( CodeFixFilter? codeFixFilter = null )
        {
            this._codeFixFilter = codeFixFilter ?? (( _, _ ) => false);
        }

        internal void Reset()
        {
            this._diagnostics = null;
            this._suppressions = null;
            this._codeFixes = null;
        }

        public void Report( Diagnostic diagnostic )
        {
            LazyInitializer.EnsureInitialized( ref this._diagnostics ).Add( diagnostic );

            if ( diagnostic.Severity == DiagnosticSeverity.Error )
            {
                Interlocked.Increment( ref this._errorCount );
            }
        }

        /// <summary>
        /// Returns a string containing all code fix titles and captures the code fixes if we should.  
        /// </summary>
        private CodeFixTitles ProcessCodeFix( IDiagnosticDefinition diagnosticDefinition, Location? location, ImmutableArray<CodeFix> codeFixes )
        {
            if ( !codeFixes.IsDefaultOrEmpty && this._codeFixAvailability != CodeFixAvailability.None )
            {
                var codeFixCreator = UserCodeExecutionContext.CurrentOrNull.AssertNotNull().Description;

                // This code implements an optimization to avoid allocating a StringBuilder if there is a single code fix. 
                string? firstTitle = null;
                StringBuilder? stringBuilder = null;

                // Store the code fixes if we should.
                foreach ( var codeFix in codeFixes )
                {
                    if ( location != null && this._codeFixFilter( diagnosticDefinition, location ) )
                    {
                        LazyInitializer.EnsureInitialized( ref this._codeFixes )
                            .Add(
                                new CodeFixInstance(
                                    codeFix,
                                    codeFixCreator,
                                    this._codeFixAvailability == CodeFixAvailability.PreviewAndApply ) );
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

        private void Suppress( ScopedSuppression suppression )
        {
            LazyInitializer.EnsureInitialized( ref this._suppressions ).Add( suppression );
        }

        internal void Suppress( IEnumerable<ScopedSuppression> suppressions )
        {
            foreach ( var suppression in suppressions )
            {
                this.Suppress( suppression );
            }
        }

        public ImmutableUserDiagnosticList ToImmutable()
            => new( this._diagnostics?.ToImmutableArray(), this._suppressions?.ToImmutableArray(), this._codeFixes?.ToImmutableArray() );

        public override string ToString()
            => $"Diagnostics={this._diagnostics?.Count ?? 0}, Suppressions={this._suppressions?.Count ?? 0}, CodeFixes={this._codeFixes?.Count ?? 0}";

        private static Location? GetLocation( IDiagnosticLocation? location ) => ((IDiagnosticLocationImpl?) location)?.DiagnosticLocation;

        private void ValidateUserReport( IDiagnosticDefinition definition )
        {
            if ( this._diagnosticManifest != null && !this._diagnosticManifest.DefinesDiagnostic( definition.Id ) )
            {
                throw new InvalidOperationException(
                    $"The aspect cannot report the diagnostic {definition.Id} because the DiagnosticDefinition is not declared as a static field or property of a compile-time class." );
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

        void IDiagnosticSink.Report( IDiagnostic diagnostic, IDiagnosticLocation? location, IDiagnosticSource source )
        {
            this.ValidateUserReport( diagnostic.Definition );

            var resolvedLocation = GetLocation( location );
            var codeFixTitles = this.ProcessCodeFix( diagnostic.Definition, resolvedLocation, diagnostic.CodeFixes );

            this.Report( diagnostic.Definition.CreateRoslynDiagnostic( resolvedLocation, diagnostic.Arguments, source, codeFixes: codeFixTitles ) );
        }

        void IDiagnosticSink.Suppress( SuppressionDefinition suppression, IDeclaration scope, IDiagnosticSource source )
        {
            this.ValidateUserSuppression( suppression );
            this.Suppress( new ScopedSuppression( suppression, scope ) );
        }

        void IDiagnosticSink.Suggest( CodeFix codeFix, IDiagnosticLocation location, IDiagnosticSource source )
        {
            var definition = GeneralDiagnosticDescriptors.SuggestedCodeFix;
            var resolvedLocation = GetLocation( location );
            var codeFixes = this.ProcessCodeFix( definition, resolvedLocation, ImmutableArray.Create( codeFix ) );

            this.Report( definition.CreateRoslynDiagnostic( resolvedLocation, codeFixes.Value!, source, codeFixes: codeFixes ) );
        }

        internal void AddCodeFixes( IEnumerable<CodeFixInstance> codeFixes )
        {
            foreach ( var codeFix in codeFixes )
            {
                LazyInitializer.EnsureInitialized( ref this._codeFixes ).Add( codeFix );
            }
        }
    }
}