// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using System;

namespace Metalama.Framework.Engine.Utilities
{
    public static class Support
    {
        private static readonly object _initializeSync = new();

        private static bool _initialized;

        public static IServiceProvider Services { get; private set; } = new ServiceProviderBuilder().ServiceProvider;

        public static void Initialize( string caller, string? projectName = null )
        {
            static ILogger GetLogger() => GetRequiredService<ILoggerFactory>().GetLogger( "Telemetry" );

            lock ( _initializeSync )
            {
                if ( _initialized )
                {
                    GetLogger().Trace?.Log( $"Support services initialization requested from {caller}. The services are initialized already." );
                    return;
                }

                Services = new ServiceProviderBuilder()
                    .AddBackstageServices( applicationInfo: new ApplicationInfo(), projectName: projectName, addLicensing: false )
                    .ServiceProvider;

                GetLogger().Trace?.Log( $"Support services initialized upon a request from {caller}." );

                Logger.Initialize();

                _initialized = true;
            }
        }

        public static T? GetOptionalService<T>()
        {
            return (T) Services.GetService( typeof( T ) );
        }

        public static T GetRequiredService<T>()
        {
            var service = (T) Services.GetService( typeof( T ) );

            if ( service == null )
            {
                throw new InvalidOperationException( $"Failed to get service of type {typeof( T )}." );
            }

            return service;
        }

        private class ApplicationInfo : IApplicationInfo
        {
            public string Name { get; }

            public string Version { get; }

            public bool IsPrerelease { get; }

            public DateTime BuildDate { get; }

            public ProcessKind ProcessKind { get; }

            // In compile time, the long running process is identified
            // by services comming from TransformerContext.
            // See the SourceTransformer.Execute method.
            public bool IsLongRunningProcess => !MetalamaCompilerInfo.IsActive;

            public bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => ProcessUtilities.IsCurrentProcessUnattended( loggerFactory );

            public ApplicationInfo()
            {
                this.Name = "Metalama";
                this.Version = AssemblyMetadataReader.MainInstance.GetPackageVersion();
                this.IsPrerelease = this.Version.Contains( "-" );
                this.BuildDate = AssemblyMetadataReader.MainInstance.GetBuildDate();
                this.ProcessKind = ProcessUtilities.ProcessKind;
            }
        }
    }
}