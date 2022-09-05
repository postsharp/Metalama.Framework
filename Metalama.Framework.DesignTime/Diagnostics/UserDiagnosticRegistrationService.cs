// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.DesignTime.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Diagnostics
{
    /// <summary>
    /// Allows to register user diagnostics and suppressions for storage in the user profile, and read this file.
    /// </summary>
    internal class UserDiagnosticRegistrationService
    {
        // Multiple instances are needed for testing.
        private static readonly ConcurrentDictionary<IConfigurationManager, UserDiagnosticRegistrationService> _instances = new();
        private readonly UserDiagnosticRegistrationFile _registrationFile;
        private readonly IConfigurationManager _configurationManager;

        public static UserDiagnosticRegistrationService GetInstance( IServiceProvider serviceProvider )
        {
            var configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();

            return _instances.GetOrAdd(
                configurationManager,
                _ => new UserDiagnosticRegistrationService( configurationManager ) );
        }

        private UserDiagnosticRegistrationService( IConfigurationManager configurationManager )
        {
            this._configurationManager = configurationManager;
            this._registrationFile = configurationManager.Get<UserDiagnosticRegistrationFile>();
        }

        /// <summary>
        /// Gets the list of supported diagnostic and suppression descriptors, as stored in the user profile.
        /// </summary>
        /// <returns></returns>
        public (ImmutableArray<DiagnosticDescriptor> Diagnostics, ImmutableArray<SuppressionDescriptor> Suppressions) GetSupportedDescriptors()
            => (this._registrationFile.Diagnostics.Values.Select( d => d.DiagnosticDescriptor() ).ToImmutableArray(),
                this._registrationFile.Suppressions.Select( id => new SuppressionDescriptor( "Metalama." + id, id, "" ) ).ToImmutableArray());

        /// <summary>
        /// Inspects a <see cref="DesignTimePipelineExecutionResult"/> and compares the reported or suppressed diagnostics to the list of supported diagnostics
        /// and suppressions from the user profile. If some items are not supported in the user profile, add them to the user profile. 
        /// </summary>
        public void RegisterDescriptors( DesignTimePipelineExecutionResult pipelineResult )
        {
            this._configurationManager.Update<UserDiagnosticRegistrationFile>(
                f =>
                {
                    var missing = this.GetMissingDiagnostics( pipelineResult );

                    if ( missing.Diagnostics.Count > 0 || missing.Suppressions.Count > 0 )
                    {
                        foreach ( var diagnostic in missing.Diagnostics )
                        {
                            f.Diagnostics.Add( diagnostic.Id, new UserDiagnosticRegistration( diagnostic ) );
                        }

                        foreach ( var suppression in missing.Suppressions )
                        {
                            f.Suppressions.Add( suppression );
                        }
                    }
                } );
        }

        private (List<string> Suppressions, List<DiagnosticDescriptor> Diagnostics) GetMissingDiagnostics( DesignTimePipelineExecutionResult pipelineResult )
        {
            List<string> missingSuppressions = new();
            List<DiagnosticDescriptor> missingDiagnostics = new();

            foreach ( var suppression in pipelineResult.Diagnostics.DiagnosticSuppressions.Select( s => s.Definition.SuppressedDiagnosticId ).Distinct() )
            {
                if ( !this._registrationFile.Suppressions.Contains( suppression ) )
                {
                    missingSuppressions.Add( suppression );
                }
            }

            foreach ( var diagnostic in pipelineResult.Diagnostics.ReportedDiagnostics.Select( d => d.Descriptor )
                         .Distinct( DiagnosticDescriptorComparer.Instance ) )
            {
                if ( !DesignTimeDiagnosticDefinitions.StandardDiagnosticDescriptors.ContainsKey( diagnostic.Id )
                     && !this._registrationFile.Diagnostics.ContainsKey( diagnostic.Id ) )
                {
                    missingDiagnostics.Add( diagnostic );
                }
            }

            return (missingSuppressions, missingDiagnostics);
        }

        private class DiagnosticDescriptorComparer : IEqualityComparer<DiagnosticDescriptor>
        {
            public static readonly DiagnosticDescriptorComparer Instance = new();

            public bool Equals( DiagnosticDescriptor x, DiagnosticDescriptor y ) => x.Id == y.Id;

            public int GetHashCode( DiagnosticDescriptor obj ) => obj.Id.GetHashCode();
        }
    }
}