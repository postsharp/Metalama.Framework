// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using System;

namespace Metalama.Framework.Engine.Utilities
{
    public static class MetalamaDiagnosticsService
    {
        public static void Initialize(string caller, string? projectName = null)
        {
            if ( DiagnosticsService.Initialize( () => new ApplicationInfo(), caller, projectName ) )
            {
                Logger.Initialize();
            }
        }

        private sealed class ApplicationInfo : IApplicationInfo
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