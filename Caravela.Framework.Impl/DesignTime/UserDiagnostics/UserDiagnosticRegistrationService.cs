// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.UserDiagnostics
{
    /// <summary>
    /// Allows to register user diagnostics and suppressions for storage in the user profile, and read this file.
    /// </summary>
    internal class UserDiagnosticRegistrationService
    {
        private static UserDiagnosticRegistrationService? _instance;
        private readonly string _settingsFilePath;
        private UserDiagnosticRegistrationFile _registrationFile;
        
        public static UserDiagnosticRegistrationService GetInstance()
            => LazyInitializer.EnsureInitialized( ref _instance, () => new UserDiagnosticRegistrationService() )!;

        private UserDiagnosticRegistrationService()
        {
            var settingsDirectory = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "Caravela" );

            RetryHelper.Retry(
                () =>
                {
                    if ( !Directory.Exists( settingsDirectory ) )
                    {
                        Directory.CreateDirectory( settingsDirectory );
                    }
                } );

            this._settingsFilePath = Path.Combine( settingsDirectory, "userDiagnostics.json" );

            this._registrationFile = UserDiagnosticRegistrationFile.Read( this._settingsFilePath );
        }

        private void RefreshRegistrationFile()
        {
            if ( File.Exists( this._settingsFilePath ) )
            {
                if ( File.GetLastWriteTime( this._settingsFilePath ) > this._registrationFile.Timestamp )
                {
                    this._registrationFile = UserDiagnosticRegistrationFile.Read( this._settingsFilePath );
                }
            }
            else
            {
                this._registrationFile = new UserDiagnosticRegistrationFile();
            }
        }

        /// <summary>
        /// Gets the list of supported diagnostic and suppression descriptors, as stored in the user profile.
        /// </summary>
        /// <returns></returns>
        public ( ImmutableArray<DiagnosticDescriptor> Diagnostics, ImmutableArray<SuppressionDescriptor> Suppressions ) GetSupportedDescriptors()
            => (this._registrationFile.Diagnostics.Values.Select( d => d.DiagnosticDescriptor() ).ToImmutableArray(),
                this._registrationFile.Suppressions.Select( id => new SuppressionDescriptor( "Caravela." + id, id, "" ) ).ToImmutableArray());

        /// <summary>
        /// Inspects a <see cref="DesignTimeAspectPipelineResult"/> and compares the reported or suppressed diagnostics to the list of supported diagnostics
        /// and suppressions from the user profile. If some items are not supported in the user profile, add them to the user profile. 
        /// </summary>
        public void RegisterDescriptors( DesignTimeAspectPipelineResult pipelineResult )
        {
            var missing = this.GetMissingDiagnostics( pipelineResult );
            var timestamp = this._registrationFile.Timestamp;

            if ( missing.Diagnostics.Count > 0 || missing.Suppressions.Count > 0 )
            {
                using ( var mutex = MutexHelper.CreateGlobalMutex( this._settingsFilePath ) )
                {
                    mutex.WaitOne();

                    try
                    {
                        this.RefreshRegistrationFile();

                        if ( timestamp != this._registrationFile.Timestamp )
                        {
                            missing = this.GetMissingDiagnostics( pipelineResult );
                        }

                        foreach ( var diagnostic in missing.Diagnostics )
                        {
                            this._registrationFile.Diagnostics.Add( diagnostic.Id, new UserDiagnosticRegistration( diagnostic ) );
                        }

                        foreach ( var suppression in missing.Suppressions )
                        {
                            this._registrationFile.Suppressions.Add( suppression );
                        }

                        this._registrationFile.Write( this._settingsFilePath );
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        private (List<string> Suppressions, List<DiagnosticDescriptor> Diagnostics) GetMissingDiagnostics( DesignTimeAspectPipelineResult pipelineResult )
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