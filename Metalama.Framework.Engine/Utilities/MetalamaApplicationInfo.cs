// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Metalama.Compiler;
using System;

namespace Metalama.Framework.Engine.Utilities
{
    internal sealed class MetalamaApplicationInfo : IApplicationInfo
    {
        public string Name { get; }

        public string Version { get; }

        public bool IsPrerelease { get; }

        public DateTime BuildDate { get; }

        public ProcessKind ProcessKind { get; }

        // In compile time, the long running process is identified
        // by services coming from TransformerContext.
        // See the SourceTransformer.Execute method.
        public bool IsLongRunningProcess => !MetalamaCompilerInfo.IsActive;

        public bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => ProcessUtilities.IsCurrentProcessUnattended( loggerFactory );

#pragma warning disable CA1822 // Mark members as static
        public bool IsTelemetryEnabled =>
#if DEBUG
            false;
#else
            true;
#endif
#pragma warning restore CA1822 // Mark members as static

        public MetalamaApplicationInfo()
        {
            this.Name = "Metalama";
            this.Version = EngineAssemblyMetadataReader.Instance.PackageVersion;
            this.IsPrerelease = this.Version.Contains( "-" );
            this.BuildDate = EngineAssemblyMetadataReader.Instance.BuildDate;
            this.ProcessKind = ProcessUtilities.ProcessKind;
        }
    }
}