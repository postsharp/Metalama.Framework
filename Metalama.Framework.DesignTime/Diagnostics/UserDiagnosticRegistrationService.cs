// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
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

        public static UserDiagnosticRegistrationService GetInstance( GlobalServiceProvider serviceProvider )
        {
            var configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();

            return _instances.GetOrAdd(
                configurationManager,
                cm => new UserDiagnosticRegistrationService( cm ) );
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
            => (this._registrationFile.Diagnostics.SelectImmutableArray( d => d.Value.DiagnosticDescriptor() ),
                this._registrationFile.Suppressions.SelectImmutableArray( id => new SuppressionDescriptor( "Metalama." + id, id, "" ) ));

        /// <summary>
        /// Inspects a <see cref="DesignTimePipelineExecutionResult"/> and compares the reported or suppressed diagnostics to the list of supported diagnostics
        /// and suppressions from the user profile. If some items are not supported in the user profile, add them to the user profile. 
        /// </summary>
        public void RegisterDescriptors( DesignTimePipelineExecutionResult pipelineResult )
        {
            try
            {
                this._configurationManager.UpdateIf<UserDiagnosticRegistrationFile>(
                    f =>
                    {
                        var missing = GetMissingDiagnostics( f, pipelineResult );

                        return missing.Diagnostics.Count > 0 || missing.Suppressions.Count > 0;
                    },
                    f =>
                    {
                        var missing = GetMissingDiagnostics( f, pipelineResult );

                        return f with
                        {
                            Diagnostics = f.Diagnostics.AddRange(
                                missing.Diagnostics.SelectArray(
                                    d => new KeyValuePair<string, UserDiagnosticRegistration>( d.Id, new UserDiagnosticRegistration( d ) ) ) ),
                            Suppressions = f.Suppressions.AddRange( missing.Suppressions )
                        };
                    } );
            }
            catch ( Exception e )
            {
                // We swallow exceptions because we don't want to fail the pipeline in case of error here.
                this._configurationManager.Logger.Error?.Log( $"Cannot register user diagnostics and registrations: {e.Message}." );
                DesignTimeExceptionHandler.ReportException( e );
            }
        }

        private static (List<string> Suppressions, List<DiagnosticDescriptor> Diagnostics) GetMissingDiagnostics(
            UserDiagnosticRegistrationFile file,
            DesignTimePipelineExecutionResult pipelineResult )
        {
            List<string> missingSuppressions = new();
            List<DiagnosticDescriptor> missingDiagnostics = new();

            foreach ( var suppression in pipelineResult.Diagnostics.DiagnosticSuppressions.Select( s => s.Definition.SuppressedDiagnosticId )
                         .Distinct() )
            {
                if ( !file.Suppressions.Contains( suppression ) )
                {
                    missingSuppressions.Add( suppression );
                }
            }

            foreach ( var diagnostic in pipelineResult.Diagnostics.ReportedDiagnostics.Select( d => d.Descriptor )
                         .Distinct( DiagnosticDescriptorComparer.Instance ) )
            {
                if ( !DesignTimeDiagnosticDefinitions.StandardDiagnosticDescriptors.ContainsKey( diagnostic.Id )
                     && !file.Diagnostics.ContainsKey( diagnostic.Id ) )
                {
                    missingDiagnostics.Add( diagnostic );
                }
            }

            return (missingSuppressions, missingDiagnostics);
        }

        private class DiagnosticDescriptorComparer : IEqualityComparer<DiagnosticDescriptor>
        {
            public static readonly DiagnosticDescriptorComparer Instance = new();

            public bool Equals( DiagnosticDescriptor? x, DiagnosticDescriptor? y )
            {
                if ( ReferenceEquals( x, y ) )
                {
                    return true;
                }
                else if ( x == null || y == null )
                {
                    return false;
                }

                return x.Id == y.Id;
            }

            public int GetHashCode( DiagnosticDescriptor obj ) => obj.Id.GetHashCodeOrdinal();
        }
    }
}